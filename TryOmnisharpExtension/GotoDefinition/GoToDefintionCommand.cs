using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.GotoDefinition;

public class GoToDefintionCommand<T> : INavigationCommand<DecompileGotoDefinitionResponse> where T : IEntity
{
    private readonly T _typeDefinition;
    private readonly IlSpyDefinitionFinderBase<T> _ilSpyTypeFinder;
    private readonly string _assemblyFilePath;

    public GoToDefintionCommand(
        T typeDefinition,
        IlSpyDefinitionFinderBase<T> ilSpyTypeFinder,
        string assemblyFilePath)
    {
        _typeDefinition = typeDefinition;
        _ilSpyTypeFinder = ilSpyTypeFinder;
        _assemblyFilePath = assemblyFilePath;
    }
        
    public Task<ResponsePacket<DecompileGotoDefinitionResponse>> Execute()
    {
        var (ilSpyMetadataSource, sourceText) = _ilSpyTypeFinder.Find(
            _typeDefinition);
            
        ilSpyMetadataSource.AssemblyFilePath = _assemblyFilePath;
            
        var result = new DecompileGotoDefinitionResponse
        {
            Location = ilSpyMetadataSource,
            SourceText = sourceText,
            IsDecompiled = true
        };

        var response = new ResponsePacket<DecompileGotoDefinitionResponse>
        {
            Body = result,
            Success = true
        };

        return Task.FromResult(response);
    }
}