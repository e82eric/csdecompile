using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GotoDefinition;

public class GoToDefinitionCommand<T> : INavigationCommand<GotoDefinitionResponse> where T : IEntity
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
        
    public Task<ResponsePacket<GotoDefinitionResponse>> Execute()
    {
        var (ilSpyMetadataSource, sourceText) = _ilSpyTypeFinder.Find(
            _typeDefinition);
            
        ilSpyMetadataSource.AssemblyFilePath = _assemblyFilePath;
            
        var result = new GotoDefinitionResponse
        {
            Location = ilSpyMetadataSource,
            SourceText = sourceText,
        };

        var response = new ResponsePacket<GotoDefinitionResponse>
        {
            Body = result,
            Success = true
        };

        return Task.FromResult(response);
    }
}