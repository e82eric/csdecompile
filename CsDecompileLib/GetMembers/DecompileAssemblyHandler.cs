using System.Threading.Tasks;
using CsDecompileLib.GotoDefinition;
using CsDecompileLib.IlSpy;

namespace CsDecompileLib.GetMembers;

public class DecompileAssemblyHandler : HandlerBase<DecompileAssemblyRequest, DecompileGotoDefinitionResponse>
{
    private readonly DecompilerFactory _decompilerFactory;

    public DecompileAssemblyHandler(
        DecompilerFactory decompilerFactory)
    {
        _decompilerFactory = decompilerFactory;
    }
    
    public override Task<ResponsePacket<DecompileGotoDefinitionResponse>> Handle(DecompileAssemblyRequest request)
    {
        var decompile = _decompilerFactory.Get(request.AssemblyFilePath);
        var (syntaxTree, source) = decompile.DecompileWholeModule();
        var result = new DecompileGotoDefinitionResponse
        {
            Location = new DecompileAssemblyInfo
            {
                AssemblyFilePath = request.AssemblyFilePath,
                AssemblyName = request.AssemblyName,
                FileName = $"{request.AssemblyName}.cs",
                SourceText = source,
                Column = 1,
                Line = 1
            },
            SourceText = source,
        };
        
        return Task.FromResult(ResponsePacket.Ok(result));
    }
}