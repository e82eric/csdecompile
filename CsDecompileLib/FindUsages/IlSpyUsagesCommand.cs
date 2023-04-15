using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindUsages;

internal class IlSpyUsagesCommand<T, TResponse> : INavigationCommand<TResponse> where TResponse : FindImplementationsResponse, new()
{
    private readonly T _symbol;
    private readonly IlSpyUsagesFinderBase<T> _usagesFinder;
        
    public IlSpyUsagesCommand(
        T symbol,
        IlSpyUsagesFinderBase<T> usagesFinder)
    {
        _symbol = symbol;
        _usagesFinder = usagesFinder;
    }
        
    public Task<ResponsePacket<TResponse>> Execute()
    {
        var metadataSources = _usagesFinder.Run(_symbol);

        var body = new TResponse();

        foreach (var metadataSource in metadataSources)
        {
            body.Implementations.Add(metadataSource);
        }

        if (_symbol is IMember)
        {
            var m = (IMember)_symbol;

            foreach (var interfaceMember in m.ExplicitlyImplementedInterfaceMembers)
            {
                var interfaceMetadataSources = _usagesFinder.Run(
                    (T)interfaceMember);

                foreach (var metadataSource in interfaceMetadataSources)
                {
                    body.Implementations.Add(metadataSource);
                }
            }
        }
        
        var result = ResponsePacket.Ok(body);

        return Task.FromResult(result);
    }
}