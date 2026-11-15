using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;

namespace CsDecompileLib.Roslyn
{
    public class RoslynLocationCommandFactory<TCommandType>
        : INavigationCommandFactoryAsync<TCommandType, DecompiledLocationRequest>
    {
        private readonly ICsDecompileWorkspace _workspace;
        private readonly IlSpySymbolFinder _ilSpySymbolFinder;
        private readonly ICommandFactory<TCommandType> _gotoDefinitionCommandFactory;

        public RoslynLocationCommandFactory(
            ICsDecompileWorkspace workspace,
            IlSpySymbolFinder ilSpySymbolFinder,
            ICommandFactory<TCommandType> gotoDefinitionCommandFactory)
        {
            _gotoDefinitionCommandFactory = gotoDefinitionCommandFactory;
            _workspace = workspace;
            _ilSpySymbolFinder = ilSpySymbolFinder;
        }

        public async Task<TCommandType> Get(DecompiledLocationRequest request)
        {
            var document = _workspace.GetDocument(request.FileName);
            if (document == null)
            {
                var fileNotFoundCommand = _gotoDefinitionCommandFactory.GetForFileNotFound(request.FileName);
                return fileNotFoundCommand;
            }

            var projectOutputFilePath = document.Project.OutputFilePath;
            var assemblyFilePath = projectOutputFilePath;
            var roslynSymbol = await GetDefinitionSymbol(document, request.Line, request.Column);

            if (roslynSymbol == null)
            {
                var symbolNotFoundAtLocationCommand = _gotoDefinitionCommandFactory.SymbolNotFoundAtLocation(
                    request.FileName,
                    request.Line,
                    request.Column);
                return symbolNotFoundAtLocationCommand;
            }

            if (roslynSymbol.Kind == SymbolKind.Namespace)
            {
                return _gotoDefinitionCommandFactory.GetForNamespace(roslynSymbol.ToString());
            }

            TCommandType result = default;

            if (roslynSymbol.Locations.Any(location => location.IsInSource))
            {
                result = _gotoDefinitionCommandFactory.GetForInSource(roslynSymbol);
                return result;
            }

            // Roslyn knows the symbol. If it can't be matched in the decompiled assembly that is
            // reported by name rather than jumping somewhere nearby.
            var memberNotFound = _gotoDefinitionCommandFactory.GetForMemberNotFound(DescribeSymbol(roslynSymbol));

            if (roslynSymbol.ContainingType is { IsExtension: true })
            {
                var extensionMember = _ilSpySymbolFinder.FindExtensionMember(assemblyFilePath, roslynSymbol);
                return extensionMember switch
                {
                    ICSharpCode.Decompiler.TypeSystem.IProperty extensionProperty =>
                        _gotoDefinitionCommandFactory.GetForProperty(extensionProperty, projectOutputFilePath),
                    ICSharpCode.Decompiler.TypeSystem.IMethod extensionMethod =>
                        _gotoDefinitionCommandFactory.GetForMethod(extensionMethod, projectOutputFilePath),
                    _ => memberNotFound
                };
            }

            switch (roslynSymbol.Kind)
            {
                case SymbolKind.NamedType:
                    var ilSpyTypeDefinition = _ilSpySymbolFinder.FindTypeDefinition(
                        assemblyFilePath,
                        roslynSymbol.GetSymbolName());

                    if (ilSpyTypeDefinition == null)
                    {
                        return memberNotFound;
                    }

                    result = _gotoDefinitionCommandFactory.GetForType(ilSpyTypeDefinition, projectOutputFilePath);
                    break;
                case SymbolKind.Property:
                    var property = (IPropertySymbol)roslynSymbol;
                    var ilspyProperty = _ilSpySymbolFinder.FindProperty(
                        assemblyFilePath,
                        property);
                    if (ilspyProperty == null)
                    {
                        return memberNotFound;
                    }

                    result = _gotoDefinitionCommandFactory.GetForProperty(ilspyProperty, projectOutputFilePath);
                    break;
                case SymbolKind.Event:
                    var eventSymbol = (IEventSymbol)roslynSymbol;
                    var ilspyEvent = _ilSpySymbolFinder.FindEvent(
                        assemblyFilePath,
                        eventSymbol);
                    if (ilspyEvent == null)
                    {
                        return memberNotFound;
                    }

                    result = _gotoDefinitionCommandFactory.GetForEvent(ilspyEvent, projectOutputFilePath);
                    break;
                case SymbolKind.Method:
                    var method = (IMethodSymbol)roslynSymbol;

                    var ilSpyMethod = _ilSpySymbolFinder.FindMethod(
                        assemblyFilePath,
                        method);

                    if (ilSpyMethod == null)
                    {
                        return memberNotFound;
                    }

                    // A direct call to the static method behind an extension block member
                    // (Extensions.get_Twice(text)) goes to the member in the block
                    result = _ilSpySymbolFinder.MapToExtensionMember(ilSpyMethod) switch
                    {
                        ICSharpCode.Decompiler.TypeSystem.IProperty extensionProperty =>
                            _gotoDefinitionCommandFactory.GetForProperty(extensionProperty, projectOutputFilePath),
                        ICSharpCode.Decompiler.TypeSystem.IMethod mappedMethod =>
                            _gotoDefinitionCommandFactory.GetForMethod(mappedMethod, projectOutputFilePath),
                        _ => _gotoDefinitionCommandFactory.GetForMethod(ilSpyMethod, projectOutputFilePath)
                    };
                    break;
                case SymbolKind.Field:
                    //I guess the field could be defined in a base class defined in a base assembly
                    var field = (IFieldSymbol)roslynSymbol;
                    var ilSpyField = _ilSpySymbolFinder.FindField(
                        assemblyFilePath,
                        field);

                    if (ilSpyField == null)
                    {
                        return memberNotFound;
                    }

                    result = _gotoDefinitionCommandFactory.GetForField(ilSpyField, projectOutputFilePath);
                    break;
                default:
                    return memberNotFound;
            }

            return result;
        }

        // Namespace.Outer+Inner.Member, the same form ILSpy uses for the type
        private static string DescribeSymbol(ISymbol symbol)
        {
            if (symbol is INamedTypeSymbol)
            {
                return symbol.GetSymbolName();
            }

            return symbol.ContainingType == null
                ? symbol.ToDisplayString()
                : $"{symbol.GetSymbolName()}.{symbol.MetadataName}";
        }

        // Some symbols have no declaration of their own to go to. Use the nearest one that does.
        private static ISymbol GetNavigableSymbol(ISymbol symbol)
        {
            while (true)
            {
                switch (symbol)
                {
                    // using Alias = Some.Type: go to the type (or namespace) the alias stands for
                    case IAliasSymbol alias:
                        symbol = alias.Target;
                        continue;
                    // var x = new int[3]: go to int
                    case IArrayTypeSymbol arrayType:
                        symbol = arrayType.ElementType;
                        continue;
                    case IPointerTypeSymbol pointerType:
                        symbol = pointerType.PointedAtType;
                        continue;
                    // tuple.Item1 on a tuple literal: go to ValueTuple.Item1 instead of the literal
                    case IFieldSymbol { IsExplicitlyNamedTupleElement: false, ContainingType.IsTupleType: true } tupleElement
                        when !SymbolEqualityComparer.Default.Equals(
                            (tupleElement.CorrespondingTupleField ?? tupleElement).OriginalDefinition, symbol):
                        symbol = (tupleElement.CorrespondingTupleField ?? tupleElement).OriginalDefinition;
                        continue;
                    // A named argument of a method from another assembly: go to the method
                    case IParameterSymbol parameter when !parameter.Locations.Any(location => location.IsInSource):
                        symbol = parameter.ContainingSymbol;
                        continue;
                    default:
                        return symbol;
                }
            }
        }

        internal async Task<ISymbol> GetDefinitionSymbol(Document document, int line, int column)
        {
            var sourceText = await document.GetTextAsync(CancellationToken.None);
            var position = GetPositionFromLineAndOffset(sourceText, line - 1, column);
            var symbol = await SymbolFinder.FindSymbolAtPositionAsync(document, position, CancellationToken.None);

            symbol = GetNavigableSymbol(symbol);

            return symbol switch
            {
                INamespaceSymbol => symbol,
                // Always prefer the partial implementation over the definition
                IMethodSymbol { IsPartialDefinition: true, PartialImplementationPart: var impl } => impl ?? symbol,
                // Don't return property getters/settings/initers
                IMethodSymbol { AssociatedSymbol: IPropertySymbol } => null,
                _ => symbol
            };
        }

        public static int GetPositionFromLineAndOffset(SourceText text, int lineNumber, int offset)
            => text.Lines[lineNumber].Start + offset;
    }
}