using System.ComponentModel.Composition;
using System.Threading.Tasks;
using TryOmnisharpExtension.FindImplementations;

namespace TryOmnisharpExtension.FindUsages;

// [Export(typeof(INavigationCommand<T>))]
internal class IlSpyUsagesCommand<T, TResponse> : INavigationCommand<TResponse> where TResponse : FindImplementationsResponse, new()
{
    private readonly T _symbol;
    private readonly IlSpyUsagesFinderBase<T> _usagesFinder;
        
    [ImportingConstructor]
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
            DecompileInfo decompileInfo = DecompileInfoMapper.MapFromMetadataSource(metadataSource);
            result.Implementations.Add(decompileInfo);
        }
            
        return Task.FromResult(result);
    }
}
