using System.Threading.Tasks;
using TryOmnisharpExtension.FindImplementations;

namespace TryOmnisharpExtension.FindUsages;

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
        
    public Task<TResponse> Execute()
    {
        var metadataSources = _usagesFinder.Run(
            _symbol);

        var result = new TResponse();
            
        foreach (var metadataSource in metadataSources)
        {
            result.Implementations.Add(metadataSource);
        }
            
        return Task.FromResult(result);
    }
}