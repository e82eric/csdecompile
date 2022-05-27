using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension
{
    [Export]
    public class IlSpyCommandFactory<CommandType>
    {
        private readonly ICommandFactory<CommandType> _commandCommandFactory;
        private readonly IlSpySymbolFinder _symbolFinder;

        [ImportingConstructor]
        public IlSpyCommandFactory(
            IlSpySymbolFinder symbolFinder,
            ICommandFactory<CommandType> commandCommandFactory)
        {
            _symbolFinder = symbolFinder;
            _commandCommandFactory = commandCommandFactory;
        }
        
        public async Task<CommandType> Find(DecompiledLocationRequest request)
        {
            var symbolAtLocation = await _symbolFinder.FindSymbolAtLocation(
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

            return default(CommandType);
        }
    }
}
