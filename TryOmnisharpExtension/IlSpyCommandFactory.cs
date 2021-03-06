using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension
{
    [Export]
    public class IlSpyCommandFactory<TCommandType>
    {
        private readonly ICommandFactory<TCommandType> _commandCommandFactory;
        private readonly IlSpySymbolFinder _symbolFinder;

        [ImportingConstructor]
        public IlSpyCommandFactory(
            IlSpySymbolFinder symbolFinder,
            ICommandFactory<TCommandType> commandCommandFactory)
        {
            _symbolFinder = symbolFinder;
            _commandCommandFactory = commandCommandFactory;
        }
        
        public TCommandType Find(DecompiledLocationRequest request)
        {
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
