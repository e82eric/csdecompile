using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension;

public class IlSpyExternalAssembliesCommandFactory<ResponseType> where ResponseType : FindImplementationsResponse, new()
{
    private readonly IDecompilerCommandFactory<INavigationCommand<ResponseType>> _commandCommandFactory;
    private readonly IlSpySymbolFinder _symbolFinder;

    [ImportingConstructor]
    public IlSpyExternalAssembliesCommandFactory(
        IlSpySymbolFinder symbolFinder,
        IDecompilerCommandFactory<INavigationCommand<ResponseType>> commandCommandFactory)
    {
        _symbolFinder = symbolFinder;
        _commandCommandFactory = commandCommandFactory;
    }
        
    public async Task<INavigationCommand<ResponseType>> Find(DecompiledLocationRequest request)
    {
        var symbolAtLocation = await _symbolFinder.FindSymbolAtLocation(
            request.AssemblyFilePath,
            request.ContainingTypeFullName,
            request.Line,
            request.Column);

        if (symbolAtLocation is ITypeDefinition entity)
        {
            var ilSpyCommand = _commandCommandFactory.GetForType(
                entity,
                request.AssemblyFilePath);

            return ilSpyCommand;
        }
            
        if (symbolAtLocation is IProperty property)
        {
            var ilSpyCommand = _commandCommandFactory.GetForProperty(property, request.AssemblyFilePath);
            return ilSpyCommand;
        }
        
        if (symbolAtLocation is IEvent eventSymbol)
        {
            var ilSpyCommand = _commandCommandFactory.GetForEvent(eventSymbol, request.AssemblyFilePath);
            return ilSpyCommand;
        }

        if(symbolAtLocation is IMethod method)
        {
            var ilSpyCommand = _commandCommandFactory.GetForMethod(method, request.AssemblyFilePath);
            return ilSpyCommand;
        }

        return null;
    }
}