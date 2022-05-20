using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using IlSpy.Analyzer.Extraction;
using TryOmnisharpExtension.FindUsages;

namespace TryOmnisharpExtension;

[Export(typeof(INavigationCommand<FindImplementationsResponse>))]
internal class FindPropertyUsagesCommand : INavigationCommand<FindUsagesResponse>
{
    private readonly IProperty _method;
    private readonly IlSpyPropertyUsagesFinder _usagesFinder;
        
    [ImportingConstructor]
    public FindPropertyUsagesCommand(
        string projectAssemblyFilePath,
        IProperty method,
        IlSpyPropertyUsagesFinder usagesFinder)
    {
        _method = method;
        _usagesFinder = usagesFinder;
    }
        
    public async Task<FindUsagesResponse> Execute()
    {
        var metadataSources = await _usagesFinder.Run(
            _method);

        var result = new FindUsagesResponse();
            
        foreach (var metadataSource in metadataSources)
        {
            DecompileInfo decompileInfo = DecompileInfoMapper.MapFromMetadataSource(metadataSource);
            result.Implementations.Add(decompileInfo);
        }
            
        return result;
    }
}