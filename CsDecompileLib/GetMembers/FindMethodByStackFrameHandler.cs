using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsDecompileLib.GetMembers;

public class FindMethodByStackFrameHandler : HandlerBase<FindMethodByStackFrameRequest, FindImplementationsResponse>
{
    private readonly FindMethodByNameHandler _findMethodByNameHandler;

    public FindMethodByStackFrameHandler(
        FindMethodByNameHandler findMethodByNameHandler)
    {
        _findMethodByNameHandler = findMethodByNameHandler;
    }

    public override async Task<ResponsePacket<FindImplementationsResponse>> Handle(
        FindMethodByStackFrameRequest request)
    {
        Regex regex = new Regex(@"^(?<namespace>.*)\.(?<class>.*)\.(?<method>.*)\(.*");
        var regexResult = regex.Match(request.StackFrame);
        if (regexResult.Success)
        {
            var findMethodByNameRequest = new FindMethodByNameRequest
            {
                NamespaceName = regexResult.Groups["namespace"].Value,
                TypeName = regexResult.Groups["class"].Value,
                MethodName = regexResult.Groups["method"].Value,
            };
            var handle = _findMethodByNameHandler.Handle(findMethodByNameRequest);
            var responsePacket = await handle;
            var result = responsePacket;

            return result;
        }

        return new ResponsePacket<FindImplementationsResponse>
        {
            Body = new FindImplementationsResponse()
        };
    }
}