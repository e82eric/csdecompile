using System.Threading.Tasks;
using CsDecompileLib.GetSource;

namespace CsDecompileLib.DebugClientAssemblySource;

public class DecompileTaskFrameHandler
    : HandlerBase<DecompileTaskFrameRequest, DecompiledSourceResponse>
{
    private readonly TaskFrameDecompiler _decompiler;

    public DecompileTaskFrameHandler(TaskFrameDecompiler decompiler)
    {
        _decompiler = decompiler;
    }
    public override Task<ResponsePacket<DecompiledSourceResponse>> Handle(DecompileTaskFrameRequest request)
    {
        var result = _decompiler.Get(request.InstructionPointer);
        return Task.FromResult(ResponsePacket.Ok(result));
    }
}