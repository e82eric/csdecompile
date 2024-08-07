using System.Threading.Tasks;
using CsDecompileLib.GetSource;

namespace CsDecompileLib.DebugClientAssemblySource;

public class DecompileFrameHandler
    : HandlerBase<DecompileFrameRequest, DecompiledSourceResponse>
{
    private readonly FrameDecompiler _frameDecompiler;

    public DecompileFrameHandler(FrameDecompiler frameDecompiler)
    {
        _frameDecompiler = frameDecompiler;
    }
    public override Task<ResponsePacket<DecompiledSourceResponse>> Handle(DecompileFrameRequest request)
    {
        var result = _frameDecompiler.Get(request.StackPointer);
        return Task.FromResult(ResponsePacket.Ok(result));
    }
}