using System.Threading.Tasks;

namespace CsDecompileLib.DebugClientAssemblySource;

public class UniqTaskCallStackHandler
    : HandlerBase<UniqTaskStacksRequest, UniqTaskStackResponse>
{
    private readonly UniqTaskCallStackProvider _uniqCallStackProvider;

    public UniqTaskCallStackHandler(UniqTaskCallStackProvider uniqCallStackProvider)
    {
        _uniqCallStackProvider = uniqCallStackProvider;
    }
    public override Task<ResponsePacket<UniqTaskStackResponse>> Handle(UniqTaskStacksRequest request)
    {
        var result = _uniqCallStackProvider.Get();
        var body = new UniqTaskStackResponse { Result = result, Success = true };
        return Task.FromResult(ResponsePacket.Ok(body));
    }
}