using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension;

internal class GotoFieldDefintionCommand : IGotoDefinitionCommand
{
    private readonly IField _field;
    private readonly IlSpyFieldFinder _ilSpyFieldFinder;
    private readonly string _projectAssemblyPath;

    public GotoFieldDefintionCommand(
        IField field,
        IlSpyFieldFinder ilSpyFieldFinder,
        string projectAssemblyPath)
    {
        _ilSpyFieldFinder = ilSpyFieldFinder;
        _projectAssemblyPath = projectAssemblyPath;
        _field = field;
    }
        
    public Task<DecompileGotoDefinitionResponse> Execute()
    {
        var (ilSpyMetadataSource, sourceText) = _ilSpyFieldFinder.Run(_field);
            
        var decompileInfo = DecompileInfoMapper.MapFromMetadataSource(ilSpyMetadataSource);
        decompileInfo.AssemblyFilePath = _projectAssemblyPath;

        decompileInfo.Line -= 1;
        decompileInfo.Column -= 1;
            
        var result = new DecompileGotoDefinitionResponse
        {
            Location = decompileInfo,
            SourceText = sourceText,
            IsDecompiled = true
        };

        return Task.FromResult(result);
    }
}