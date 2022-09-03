using CsDecompileLib.GotoDefinition;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using CsDecompileLib.FindUsages;

namespace CsDecompileLib
{
    public class IlSpyCommandFactory<TCommandType>
    {
        private readonly ICommandFactory<TCommandType> _commandCommandFactory;
        private readonly IlSpySymbolFinder _symbolFinder;

        public IlSpyCommandFactory(
            IlSpySymbolFinder symbolFinder,
            ICommandFactory<TCommandType> commandCommandFactory)
        {
            _symbolFinder = symbolFinder;
            _commandCommandFactory = commandCommandFactory;
        }
        
        public TCommandType Find(DecompiledLocationRequest request)
        {
            var containingTypeDefinition = _symbolFinder.FindTypeDefinition(
                request.AssemblyFilePath,
                request.ContainingTypeFullName);
            
             (AstNode node, SyntaxTree syntaxTree, string sourceText) findNodeResult = _symbolFinder.FindNode(
                containingTypeDefinition,
                request.Line,
                request.Column);

            var parentResolveResult = findNodeResult.node.Parent.GetResolveResult();

            //TODO: This should be in a usages specific context
            if (parentResolveResult is ILVariableResolveResult variableResolveResult)
            {
                var variableFinder = new VariableInTypeFinder();
                variableFinder.Find(variableResolveResult.Variable, findNodeResult.syntaxTree);

                var variableCommand = _commandCommandFactory.GetForVariable(
                    variableResolveResult.Variable,
                    containingTypeDefinition,
                    findNodeResult.syntaxTree,
                    findNodeResult.sourceText,
                    request.AssemblyFilePath);
                return variableCommand;
            }
            
            var symbolAtLocation = _symbolFinder.FindSymbolAtLocation(
                request.AssemblyFilePath,
                request.ContainingTypeFullName,
                request.Line,
                request.Column);

            if (symbolAtLocation is ITypeDefinition entity)
            {
                return _commandCommandFactory.GetForType(
                    entity,
                    request.AssemblyFilePath);
            }
            
            if (symbolAtLocation is IProperty property)
            {
                var result = _commandCommandFactory.GetForProperty(property, request.AssemblyFilePath);
                return result;
            }
            
            if (symbolAtLocation is IEvent eventSymbol)
            {
                var result = _commandCommandFactory.GetForEvent(eventSymbol, request.AssemblyFilePath);
                return result;
            }

            if(symbolAtLocation is IMethod method)
            {
                var result = _commandCommandFactory.GetForMethod(method, request.AssemblyFilePath);
                return result;
            }
            
            if(symbolAtLocation is IField field)
            {
                var result = _commandCommandFactory.GetForField(field, request.AssemblyFilePath);
                return result;
            }

            return default;
        }
    }
}
