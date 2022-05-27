using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension;

internal class FindEventImplementationsCommand : IFindImplementationsCommand
{
    private readonly IlSpyEventImplementationFinder _ilSpyImplementationFinder;
    private readonly string _projectAssemblyFilePath;
    private readonly IEvent _symbol;

    public FindEventImplementationsCommand(
        string projectAssemblyFilePath,
        IEvent symbol,
        IlSpyEventImplementationFinder ilSpyImplementationFinder)
    {
        _projectAssemblyFilePath = projectAssemblyFilePath;
        _symbol = symbol;
        _ilSpyImplementationFinder = ilSpyImplementationFinder;
    }
        
    public async Task<FindImplementationsResponse> Execute()
    {
        var metadataSources = await _ilSpyImplementationFinder.Run(
            _symbol,
            _projectAssemblyFilePath);

        var result = new FindImplementationsResponse();
            
        foreach (var metadataSource in metadataSources)
        {
            DecompileInfo decompileInfo = DecompileInfoMapper.MapFromMetadataSource(metadataSource);
            result.Implementations.Add(decompileInfo);
        }
        
        return result;
    }
}