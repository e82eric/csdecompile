using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using OmniSharp;
using OmniSharp.Extensions;

namespace TryOmnisharpExtension;

[Export]
public class EverywhereSymbolInfoFinder
{
    private readonly OmniSharpWorkspace _workspace;
    private readonly IlSpySymbolFinder _ilSpySymbolFinder;
    private readonly ICommandFactory<IFindImplementationsCommand> _commandFactory;

    [ImportingConstructor]
    public EverywhereSymbolInfoFinder(
        OmniSharpWorkspace workspace,
        IlSpySymbolFinder ilSpySymbolFinder,
        ICommandFactory<IFindImplementationsCommand> commandFactory)
    {
        _commandFactory = commandFactory;
        _workspace = workspace;
        _ilSpySymbolFinder = ilSpySymbolFinder;
    }
        
    public async Task<IFindImplementationsCommand> Get(LocationRequest request)
    {
        var document = _workspace.GetDocument(request.FileName);
        var projectOutputFilePath = document.Project.OutputFilePath;
        var assemblyFilePath = projectOutputFilePath;
        var roslynSymbol = await GetDefinitionSymbol(document, request.Line, request.Column);
            
        var rosylnCommand = _commandFactory.GetForInSource(roslynSymbol);
            
        if (roslynSymbol.Locations.First().IsInSource)
        {
            return rosylnCommand;
        }
            
        IFindImplementationsCommand ilSpyCommand = default;
        switch (roslynSymbol.Kind)
        {
            case SymbolKind.NamedType:
                var ilSpyTypeDefinition = await _ilSpySymbolFinder.FindTypeDefinition(
                    assemblyFilePath,
                    roslynSymbol.GetSymbolName());
                    
                ilSpyCommand = _commandFactory.GetForType(ilSpyTypeDefinition, projectOutputFilePath);
                break;
            case SymbolKind.Property:
                var property = (IPropertySymbol)roslynSymbol;
                var propertyName = $"{property.ContainingType.GetSymbolName()}.{property.Name}";
                var ilspyProperty = await _ilSpySymbolFinder.FindProperty(
                    assemblyFilePath,
                    property.ContainingType.GetSymbolName(),
                    propertyName);
                ilSpyCommand = _commandFactory.GetForProperty(ilspyProperty, projectOutputFilePath);
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

                var fullName = $"{method.ContainingType.GetSymbolName()}.{method.Name}";

                var ilSpyMethod = await _ilSpySymbolFinder.FindMethod(
                    assemblyFilePath,
                    method);

                ilSpyCommand = _commandFactory.GetForMethod(ilSpyMethod, projectOutputFilePath);
                break;
            default:
                throw new Exception();
        }

        var result = new EverywhereImplementationsCommand(rosylnCommand, ilSpyCommand);
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
}



[Export]
public class EverywhereSymbolInfoFinder2<CommandResponseType> where CommandResponseType : FindImplementationsResponse, new()
{
    private readonly OmniSharpWorkspace _workspace;
    private readonly IlSpySymbolFinder _ilSpySymbolFinder;
    private readonly ICommandFactory<INavigationCommand<CommandResponseType>> _commandFactory;

    [ImportingConstructor]
    public EverywhereSymbolInfoFinder2(
        OmniSharpWorkspace workspace,
        IlSpySymbolFinder ilSpySymbolFinder,
        ICommandFactory<INavigationCommand<CommandResponseType>> commandFactory)
    {
        _commandFactory = commandFactory;
        _workspace = workspace;
        _ilSpySymbolFinder = ilSpySymbolFinder;
    }
        
    public async Task<INavigationCommand<CommandResponseType>> Get(LocationRequest request)
    {
        var document = _workspace.GetDocument(request.FileName);
        var projectOutputFilePath = document.Project.OutputFilePath;
        var assemblyFilePath = projectOutputFilePath;
        var roslynSymbol = await GetDefinitionSymbol(document, request.Line, request.Column);
            
        var rosylnCommand = _commandFactory.GetForInSource(roslynSymbol);
            
        if (roslynSymbol.Locations.First().IsInSource)
        {
            return rosylnCommand;
        }
            
        INavigationCommand<CommandResponseType> ilSpyCommand = default;
        switch (roslynSymbol.Kind)
        {
            case SymbolKind.NamedType:
                var ilSpyTypeDefinition = await _ilSpySymbolFinder.FindTypeDefinition(
                    assemblyFilePath,
                    roslynSymbol.GetSymbolName());
                    
                ilSpyCommand = _commandFactory.GetForType(ilSpyTypeDefinition, projectOutputFilePath);
                break;
            case SymbolKind.Property:
                var property = (IPropertySymbol)roslynSymbol;
                var propertyName = $"{property.ContainingType.GetSymbolName()}.{property.Name}";
                var ilspyProperty = await _ilSpySymbolFinder.FindProperty(
                    assemblyFilePath,
                    property.ContainingType.GetSymbolName(),
                    propertyName);
                ilSpyCommand = _commandFactory.GetForProperty(ilspyProperty, projectOutputFilePath);
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

                var fullName = $"{method.ContainingType.GetSymbolName()}.{method.Name}";

                var ilSpyMethod = await _ilSpySymbolFinder.FindMethod(
                    assemblyFilePath,
                    method);

                ilSpyCommand = _commandFactory.GetForMethod(ilSpyMethod, projectOutputFilePath);
                break;
            default:
                throw new Exception();
        }

        var result = new EverywhereImplementationsCommand2<CommandResponseType>(rosylnCommand, ilSpyCommand);
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
}
