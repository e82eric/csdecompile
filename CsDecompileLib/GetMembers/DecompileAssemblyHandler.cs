using System.Threading.Tasks;
using CsDecompileLib.GotoDefinition;
using CsDecompileLib.IlSpy;

namespace CsDecompileLib.GetMembers;

public class DecompileAssemblyHandler : HandlerBase<DecompileAssemblyRequest, DecompileAssemlbyResponse>
{
    private readonly DecompilerFactory _decompilerFactory;

    public DecompileAssemblyHandler(
        DecompilerFactory decompilerFactory)
    {
        _decompilerFactory = decompilerFactory;
    }
    
    public override Task<ResponsePacket<DecompileAssemlbyResponse>> Handle(DecompileAssemblyRequest request)
    {
        var decompile = _decompilerFactory.Get(request.AssemblyFilePath);
        var (_, source) = decompile.DecompileWholeModule();
        var result = new DecompileAssemlbyResponse
        {
            SourceText = source,
        };
        
        return Task.FromResult(ResponsePacket.Ok(result));
    }
}