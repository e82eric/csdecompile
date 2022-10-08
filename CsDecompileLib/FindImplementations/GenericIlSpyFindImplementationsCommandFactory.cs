using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindImplementations;

public class GenericIlSpyFindImplementationsCommandFactory<TResponseType>
    where TResponseType : FindImplementationsResponse, new()
{
    private readonly IVariableCommandProvider<INavigationCommand<TResponseType>> _variableCommandProvider;
    private readonly EveryWhereFindImplementationsCommandFactory<TResponseType> _everwhereCommandFactory;

    public GenericIlSpyFindImplementationsCommandFactory(
        IVariableCommandProvider<INavigationCommand<TResponseType>> variableCommandProvider,
        EveryWhereFindImplementationsCommandFactory<TResponseType> everwhereCommandFactory)
    {
        _variableCommandProvider = variableCommandProvider;
        _everwhereCommandFactory = everwhereCommandFactory;
    }

    public Task<INavigationCommand<TResponseType>> Find(DecompiledLocationRequest request)
    {
        INavigationCommand<TResponseType> result = null;
        var (isVariableCommand, variableCommand, symbolAtLocation) = _variableCommandProvider.GetNodeInformation(request);
        if (isVariableCommand)
        {
            result = variableCommand;
        }
        else if (symbolAtLocation is ITypeDefinition entity)
        {
            result = _everwhereCommandFactory.GetForType(entity, request.AssemblyFilePath);
        }
        else if (symbolAtLocation is IProperty property)
        {
            result = _everwhereCommandFactory.GetForProperty(property, request.AssemblyFilePath);
        }
        else if (symbolAtLocation is IField field)
        {
            result = _everwhereCommandFactory.GetForField(field, request.AssemblyFilePath);
        }
        else if (symbolAtLocation is IEvent eventSymbol)
        {
            result = _everwhereCommandFactory.GetForEvent(eventSymbol, request.AssemblyFilePath);
        }
        else if (symbolAtLocation is IMethod method)
        {
            result = _everwhereCommandFactory.GetForMethod(method, request.AssemblyFilePath);
        }
        
        return Task.FromResult(result);
    }
}