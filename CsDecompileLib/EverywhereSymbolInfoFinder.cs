using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsDecompileLib.FindImplementations;
using CsDecompileLib.IlSpy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using CsDecompileLib.Roslyn;
using TypeKind = ICSharpCode.Decompiler.TypeSystem.TypeKind;

namespace CsDecompileLib;

public class EverywhereSymbolInfoFinder<TCommandResponseType>
    : INavigationCommandFactoryAsync<INavigationCommand<TCommandResponseType>, DecompiledLocationRequest>
    where TCommandResponseType :FindImplementationsResponse, new()
{
    private readonly IOmniSharpWorkspace _workspace;
    private readonly IlSpySymbolFinder _ilSpySymbolFinder;
    private readonly ICommandFactory<INavigationCommand<TCommandResponseType>> _commandFactory;

    public EverywhereSymbolInfoFinder(
        IOmniSharpWorkspace workspace,
        IlSpySymbolFinder ilSpySymbolFinder,
        ICommandFactory<INavigationCommand<TCommandResponseType>> commandFactory)
    {
        _commandFactory = commandFactory;
        _workspace = workspace;
        _ilSpySymbolFinder = ilSpySymbolFinder;
    }
        
    public async Task<INavigationCommand<TCommandResponseType>> Get(DecompiledLocationRequest request)
    {
        var document = _workspace.GetDocument(request.FileName);
        if (document == null)
        {
            var fileNotFoundCommand = _commandFactory.GetForFileNotFound(request.FileName);
            return fileNotFoundCommand;
        }
        var projectOutputFilePath = document.Project.OutputFilePath;
        var assemblyFilePath = projectOutputFilePath;
        var roslynSymbol = await GetDefinitionSymbol(document, request.Line, request.Column);
            
        if (roslynSymbol == null)
        {
            var symbolNotFoundAtLocationCommand = _commandFactory.SymbolNotFoundAtLocation(
                request.FileName,
                request.Line,
                request.Column);
            return symbolNotFoundAtLocationCommand;
        }
        
        var rosylnCommand = _commandFactory.GetForInSource(roslynSymbol);
            
        if (roslynSymbol.Locations.First().IsInSource)
        {
            return rosylnCommand;
        }
            
        INavigationCommand<TCommandResponseType> ilSpyCommand = default;
        switch (roslynSymbol.Kind)
        {
            case SymbolKind.NamedType:
                var ilSpyTypeDefinition = _ilSpySymbolFinder.FindTypeDefinition(
                    assemblyFilePath,
                    roslynSymbol.GetSymbolName());
                    
                ilSpyCommand = _commandFactory.GetForType(ilSpyTypeDefinition, projectOutputFilePath);
                break;
            case SymbolKind.Property:
                var property = (IPropertySymbol)roslynSymbol;
                var propertyName = $"{property.ContainingType.GetSymbolName()}.{property.Name}";
                var ilspyProperty = _ilSpySymbolFinder.FindProperty(
                    assemblyFilePath,
                    property.ContainingType.GetSymbolName(),
                    propertyName);
                ilSpyCommand = _commandFactory.GetForProperty(ilspyProperty, projectOutputFilePath);
                break;
            case SymbolKind.Event:
                var @event = (IEventSymbol)roslynSymbol;
                var eventName = $"{@event.ContainingType.GetSymbolName()}.{@event.Name}";
                var ilspyEvent = _ilSpySymbolFinder.FindEvent(
                    assemblyFilePath,
                    @event.ContainingType.GetSymbolName(),
                    eventName);
                ilSpyCommand = _commandFactory.GetForEvent(ilspyEvent, projectOutputFilePath);
                break;
            case SymbolKind.Field:
                var fieldSymbol = (IFieldSymbol)roslynSymbol;
                var ilspyField = _ilSpySymbolFinder.FindField(
                    assemblyFilePath,
                    fieldSymbol);
                if (ilspyField.DeclaringType.Kind == TypeKind.Enum)
                {
                    ilSpyCommand = _commandFactory.GetForEnumField(ilspyField, projectOutputFilePath);
                }
                else
                {
                    ilSpyCommand = _commandFactory.GetForField(ilspyField, projectOutputFilePath);
                }
                break;
            case SymbolKind.Method:
                var method = (IMethodSymbol)roslynSymbol;
                var methodParameters = new List<string>();

                if (method.ReducedFrom != null)
                {
                    foreach (var parameter in method.ReducedFrom.Parameters)
                    {
                        methodParameters.Add(parameter.Type.GetMetadataName());
                    }
                }
                else
                {
                    foreach (var parameter in method.Parameters)
                    {
                        methodParameters.Add(parameter.Type.GetMetadataName());
                    }
                }

                var ilSpyMethod = _ilSpySymbolFinder.FindMethod(
                    assemblyFilePath,
                    method);

                ilSpyCommand = _commandFactory.GetForMethod(ilSpyMethod, projectOutputFilePath);
                break;
            default:
                throw new Exception();
        }

        var result = new EverywhereImplementationsCommand2<TCommandResponseType>(rosylnCommand, ilSpyCommand);
        return result;
    }
        
    internal async Task<ISymbol> GetDefinitionSymbol(Document document, int line, int column)
    {
        var sourceText = await document.GetTextAsync(CancellationToken.None);
        var position = GetPositionFromLineAndOffset(sourceText, line -1, column);
        var symbol = await SymbolFinder.FindSymbolAtPositionAsync(document, position, CancellationToken.None);

        return symbol switch
        {
            INamespaceSymbol => null,
            // Always prefer the partial implementation over the definition
            IMethodSymbol { IsPartialDefinition: true, PartialImplementationPart: var impl } => impl,
            // Don't return property getters/settings/initers
            IMethodSymbol { AssociatedSymbol: IPropertySymbol } => null,
            _ => symbol
        };
    }

    public static int GetPositionFromLineAndOffset(SourceText text, int lineNumber, int offset)
        => text.Lines[lineNumber].Start + offset;

    public TCommandResponseType Find(DecompiledLocationRequest request)
    {
        throw new NotImplementedException();
    }
}
