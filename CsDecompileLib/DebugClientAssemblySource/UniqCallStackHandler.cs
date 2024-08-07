using System.Threading.Tasks;

namespace CsDecompileLib.DebugClientAssemblySource;

public class UniqCallStackHandler
    : HandlerBase<UniqCallStacksRequest, UniqCallStackResponse>
{
    private readonly UniqCallStackProvider _uniqCallStackProvider;

    public UniqCallStackHandler(UniqCallStackProvider uniqCallStackProvider)
    {
        _uniqCallStackProvider = uniqCallStackProvider;
    }
    public override Task<ResponsePacket<UniqCallStackResponse>> Handle(UniqCallStacksRequest request)
    {
        var result = _uniqCallStackProvider.Get();
        var body = new UniqCallStackResponse { Result = result, Success = true };
        return Task.FromResult(ResponsePacket.Ok(body));
    }
}