using System.ComponentModel.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using IlSpy.Analyzer.Extraction;
using TryOmnisharpExtension.FindUsages;

namespace TryOmnisharpExtension;

[Export(typeof(INavigationCommand<FindImplementationsResponse>))]
internal class FindEventUsagesCommand : INavigationCommand<FindUsagesResponse>
{
    private readonly IEvent _symbol;
    private readonly IlSpyEventUsagesFinder _usagesFinder;
        
    [ImportingConstructor]
    public FindEventUsagesCommand(
        string projectAssemblyFilePath,
        IEvent symbol,
        IlSpyEventUsagesFinder usagesFinder)
    {
        _symbol = symbol;
        _usagesFinder = usagesFinder;
    }
        
    public async Task<FindUsagesResponse> Execute()
    {
        var metadataSources = await _usagesFinder.Run(
            _symbol);

        var result = new FindUsagesResponse();
            
        foreach (var metadataSource in metadataSources)
        {
            DecompileInfo decompileInfo = DecompileInfoMapper.MapFromMetadataSource(metadataSource);
            result.Implementations.Add(decompileInfo);
        }
            
        return result;
    }
}