using System.Collections.Generic;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension;

internal class FindMethodImplementationsCommand : IFindImplementationsCommand
{
    private readonly IlSpyMethodImplementationFinder _ilSpyTypeFinder;
    private readonly string _projectAssemblyFilePath;
    private readonly IMethod _typeDefinition;

    public FindMethodImplementationsCommand(
        string projectAssemblyFilePath,
        IMethod typeDefinition,
        IlSpyMethodImplementationFinder ilSpyTypeFinder)
    {
        _projectAssemblyFilePath = projectAssemblyFilePath;
        _typeDefinition = typeDefinition;
        _ilSpyTypeFinder = ilSpyTypeFinder;
    }
        
    public async Task<FindImplementationsResponse> Execute()
    {
        var metadataSources = await _ilSpyTypeFinder.Run(
            _typeDefinition,
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