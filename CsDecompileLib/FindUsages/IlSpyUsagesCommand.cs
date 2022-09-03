using System.Threading.Tasks;

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
        var metadataSources = _usagesFinder.Run(
            _symbol);

        var body = new TResponse();
            
        foreach (var metadataSource in metadataSources)
        {
            body.Implementations.Add(metadataSource);
        }

        var result = ResponsePacket.Ok(body);
            
        return Task.FromResult(result);
    }
}