using CsDecompileLib.FindImplementations;
using CsDecompileLib.GotoDefinition;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib
{
    public class IlSpyCommandFactory<TCommandType> : INavigationCommandFactory<TCommandType>
    {
        private readonly IVariableCommandProvider<TCommandType> _variableCommandProvider;
        private readonly ICommandFactory<TCommandType> _commandFactory;
        private readonly IlSpySymbolFinder _symbolFinder;

        public IlSpyCommandFactory(
            IVariableCommandProvider<TCommandType> variableCommandProvider,
            ICommandFactory<TCommandType> commandFactory,
            IlSpySymbolFinder symbolFinder)
        {
            _variableCommandProvider = variableCommandProvider;
            _commandFactory = commandFactory;
            _symbolFinder = symbolFinder;
        }
        
        public TCommandType Find(DecompiledLocationRequest request)
        {
            TCommandType result = default;
            var (isVariable, variableCommand, node) = _variableCommandProvider.GetNodeInformation(request);
            if (isVariable)
            {
                result = variableCommand;
            }
            else if (node == null)
            {
                result = _commandFactory.SymbolNotFoundAtLocation(
                    request.FileName ?? request.ContainingTypeFullName,
                    request.Line,
                    request.Column);
            }
            else
            {
                if(node.Parent?.Slot?.Name is "Import" or "Target")
                {
                    var namespaceString = node.Parent.ToString();
                    var currentNode = node.Parent;
                    while (currentNode.Parent != null)
                    {
                        if (currentNode is UsingDeclaration usingNode)
                        {
                            return _commandFactory.GetForNamespace(namespaceString);
                        }

                        currentNode = currentNode.Parent;
                    }
                }

                // namespace Some.Name; at the top of the decompiled file. The decompiler attaches
                // no symbol to it.
                var declaredNamespace = GetDeclaredNamespace(node);
                if (declaredNamespace != null)
                {
                    return _commandFactory.GetForNamespace(declaredNamespace);
                }

                var symbolAtLocation = _symbolFinder.FindSymbolFromNode(node);
                switch (symbolAtLocation)
                {
                    case ITypeDefinition entity:
                        result = _commandFactory.GetForType(entity, request.AssemblyFilePath);
                        break;
                    case IProperty property:
                        result = _commandFactory.GetForProperty(property, request.AssemblyFilePath);
                        break;
                    case IEvent eventSymbol:
                        result = _commandFactory.GetForEvent(eventSymbol, request.AssemblyFilePath);
                        break;
                    case IMethod method:
                        result = _commandFactory.GetForMethod(method, request.AssemblyFilePath);
                        break;
                    case IField field:
                        result = _commandFactory.GetForField(field, request.AssemblyFilePath);
                        break;
                    // Nothing that can be navigated to (a literal, a symbol kind that isn't handled)
                    default:
                        result = _commandFactory.SymbolNotFoundAtLocation(
                            request.FileName ?? request.ContainingTypeFullName,
                            request.Line,
                            request.Column);
                        break;
                }
            }

            return result;
        }

        // The namespace named by the node when it is part of the name of a namespace declaration:
        // in "namespace Some.Name;" that is Some for the first part and Some.Name for the second.
        private static string GetDeclaredNamespace(AstNode node)
        {
            if (node.Parent is not AstType namePart)
            {
                return null;
            }

            AstNode current = namePart;
            while (current is AstType)
            {
                current = current.Parent;
            }

            return current is NamespaceDeclaration ? namePart.ToString() : null;
        }
    }
}