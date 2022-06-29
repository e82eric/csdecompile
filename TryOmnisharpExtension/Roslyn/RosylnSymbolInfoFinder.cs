using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.Roslyn
{
    public class RosylnSymbolInfoFinder<TCommandType>
    {
        private readonly IOmniSharpWorkspace _workspace;
        private readonly IlSpySymbolFinder _ilSpySymbolFinder;
        private readonly ICommandFactory<TCommandType> _gotoDefinitionCommandFactory;

        public RosylnSymbolInfoFinder(
            IOmniSharpWorkspace workspace,
            IlSpySymbolFinder ilSpySymbolFinder,
            ICommandFactory<TCommandType> gotoDefinitionCommandFactory)
        {
            _gotoDefinitionCommandFactory = gotoDefinitionCommandFactory;
            _workspace = workspace;
            _ilSpySymbolFinder = ilSpySymbolFinder;
        }
        
        public async Task<TCommandType> Get(LocationRequest request)
        {
            var document = _workspace.GetDocument(request.FileName);
            var projectOutputFilePath = document.Project.OutputFilePath;
            var assemblyFilePath = projectOutputFilePath;
            var roslynSymbol = await GetDefinitionSymbol(document, request.Line, request.Column);

            TCommandType result = default;
            
            if (roslynSymbol.Locations.First().IsInSource)
            {
                result = _gotoDefinitionCommandFactory.GetForInSource(roslynSymbol);
                return result;
            }
            
            switch (roslynSymbol.Kind)
            {
                case SymbolKind.NamedType:
                    var ilSpyTypeDefinition = _ilSpySymbolFinder.FindTypeDefinition(
                        assemblyFilePath,
                        roslynSymbol.GetSymbolName());
                    
                    result = _gotoDefinitionCommandFactory.GetForType(ilSpyTypeDefinition, projectOutputFilePath);
                    break;
                case SymbolKind.Property:
                    var property = (IPropertySymbol)roslynSymbol;
                    var propertyName = $"{property.ContainingType.GetSymbolName()}.{property.Name}";
                    var ilspyProperty = _ilSpySymbolFinder.FindProperty(
                        assemblyFilePath,
                        property.ContainingType.GetSymbolName(),
                        propertyName);
                    result = _gotoDefinitionCommandFactory.GetForProperty(ilspyProperty, projectOutputFilePath);
                    break;
                case SymbolKind.Event:
                    var eventSymbol = (IEventSymbol)roslynSymbol;
                    var eventName = $"{eventSymbol.ContainingType.GetSymbolName()}.{eventSymbol.Name}";
                    var ilspyEvent = _ilSpySymbolFinder.FindEvent(
                        assemblyFilePath,
                        eventSymbol.ContainingType.GetSymbolName(),
                        eventName);
                    result = _gotoDefinitionCommandFactory.GetForEvent(ilspyEvent, projectOutputFilePath);
                    break;
                case SymbolKind.Method:
                    var method = (IMethodSymbol)roslynSymbol;

                    var ilSpyMethod = _ilSpySymbolFinder.FindMethod(
                        assemblyFilePath,
                        method);

                    result = _gotoDefinitionCommandFactory.GetForMethod(ilSpyMethod, projectOutputFilePath);
                    break;
                case SymbolKind.Field:
                    //I guess the field could be defined in a base class defined in a base assembly
                    var field = (IFieldSymbol)roslynSymbol;
                    var ilSpyField = _ilSpySymbolFinder.FindField(
                        assemblyFilePath,
                        field);

                    result = _gotoDefinitionCommandFactory.GetForField(ilSpyField, projectOutputFilePath);
                    break;
                default:
                    throw new Exception();
            }

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
}