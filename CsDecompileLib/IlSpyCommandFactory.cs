using CsDecompileLib.FindImplementations;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib
{
    public class IlSpyCommandFactory<TCommandType>
    {
        private readonly IVariableCommandProvider<TCommandType> _variableCommandProvider;
        private readonly ICommandFactory<TCommandType> _commandCommandFactory;

        public IlSpyCommandFactory(
            IVariableCommandProvider<TCommandType> variableCommandProvider,
            ICommandFactory<TCommandType> commandCommandFactory)
        {
            _variableCommandProvider = variableCommandProvider;
            _commandCommandFactory = commandCommandFactory;
        }
        
        public TCommandType Find(DecompiledLocationRequest request)
        {
            TCommandType result = default;
            var (isVariable, variableCommand, symbolAtLocation) = _variableCommandProvider.GetNodeInformation(request);
            if (isVariable)
            {
                result = variableCommand;
            }
            else
            {
                if (symbolAtLocation is ITypeDefinition entity)
                {
                    return _commandCommandFactory.GetForType(
                        entity,
                        request.AssemblyFilePath);
                }
                    
                if (symbolAtLocation is IProperty property)
                {
                    result = _commandCommandFactory.GetForProperty(property, request.AssemblyFilePath);
                }
                    
                if (symbolAtLocation is IEvent eventSymbol)
                {
                    result = _commandCommandFactory.GetForEvent(eventSymbol, request.AssemblyFilePath);
                }

                if(symbolAtLocation is IMethod method)
                {
                    result = _commandCommandFactory.GetForMethod(method, request.AssemblyFilePath);
                }
                    
                if(symbolAtLocation is IField field)
                {
                    result = _commandCommandFactory.GetForField(field, request.AssemblyFilePath);
                }
            }

            return result;
        }
    }
}