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
            else
            {
                if(node.Parent.Role.ToString() == "Import" || node.Parent.Role.ToString() == "Target")
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
                }
            }

            return result;
        }
    }
}