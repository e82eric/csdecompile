using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using IlSpy.Analyzer.Extraction;
using TryOmnisharpExtension.FindUsages;

namespace TryOmnisharpExtension;

[Export(typeof(INavigationCommand<FindImplementationsResponse>))]
internal class FindMethodUsagesCommand : INavigationCommand<FindUsagesResponse>
{
    private readonly IMethod _method;
    private readonly IlSpyMethodUsagesFinder _usagesFinder;
        
    [ImportingConstructor]
    public FindMethodUsagesCommand(
        string projectAssemblyFilePath,
        IMethod method,
        IlSpyMethodUsagesFinder usagesFinder)
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