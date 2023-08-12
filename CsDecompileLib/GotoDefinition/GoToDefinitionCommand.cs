using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GotoDefinition;

public class GoToDefinitionCommand<T> : INavigationCommand<FindImplementationsResponse> where T : IEntity
{
    private readonly T _typeDefinition;
    private readonly IlSpyDefinitionFinderBase<T> _ilSpyTypeFinder;
    private readonly string _assemblyFilePath;

    public GoToDefinitionCommand(
        T typeDefinition,
        IlSpyDefinitionFinderBase<T> ilSpyTypeFinder,
        string assemblyFilePath)
    {
        _typeDefinition = typeDefinition;
        _ilSpyTypeFinder = ilSpyTypeFinder;
        _assemblyFilePath = assemblyFilePath;
    }
        
    public Task<ResponsePacket<FindImplementationsResponse>> Execute()
    {
        var ilSpyMetadataSource = _ilSpyTypeFinder.Find(
            _typeDefinition);
            
        ilSpyMetadataSource.AssemblyFilePath = _assemblyFilePath;
        ilSpyMetadataSource.ParentAssemblyFilePath = _typeDefinition.ParentModule.PEFile.FileName;
            
        var result = new FindImplementationsResponse
        {
            Implementations = {  ilSpyMetadataSource },
        };

        var response = new ResponsePacket<FindImplementationsResponse>
        {
            Body = result,
            Success = true
        };

        return Task.FromResult(response);
    }
}