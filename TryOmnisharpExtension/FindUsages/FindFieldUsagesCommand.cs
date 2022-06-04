using System.ComponentModel.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using IlSpy.Analyzer.Extraction;
using TryOmnisharpExtension.FindUsages;

namespace TryOmnisharpExtension;

[Export(typeof(INavigationCommand<FindImplementationsResponse>))]
internal class FindFieldUsagesCommand : INavigationCommand<FindUsagesResponse>
{
    private readonly IField _field;
    private readonly IlSpyFieldUsagesFinder _usagesFinder;
        
    [ImportingConstructor]
    public FindFieldUsagesCommand(
        string projectAssemblyFilePath,
        IField field,
        IlSpyFieldUsagesFinder usagesFinder)
    {
        _field = field;
        _usagesFinder = usagesFinder;
    }
        
    public async Task<FindUsagesResponse> Execute()
    {
        var metadataSources = await _usagesFinder.Run(
            _field);

        var result = new FindUsagesResponse();
            
        foreach (var metadataSource in metadataSources)
        {
            DecompileInfo decompileInfo = DecompileInfoMapper.MapFromMetadataSource(metadataSource);
            result.Implementations.Add(decompileInfo);
        }
            
        return result;
    }
}