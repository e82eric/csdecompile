using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsDecompileLib.GetMembers;

public class FindMethodByStackFrameHandler : HandlerBase<FindMethodByStackFrameRequest, LocationsResponse>
{
    private readonly FindMethodByNameHandler _findMethodByNameHandler;

    public FindMethodByStackFrameHandler(
        FindMethodByNameHandler findMethodByNameHandler)
    {
        _findMethodByNameHandler = findMethodByNameHandler;
    }

    public override async Task<ResponsePacket<LocationsResponse>> Handle(
        FindMethodByStackFrameRequest request)
    {
        Regex noGenericsRegex = new Regex(@"^(?<namespace>.*)\.(?<class>.*)\.(?<method>.*)\(.*");
        Regex genericMethodRegex = new Regex(@"^(?<namespace>.*)\.(?!<)(?<class>.*)\.(?<method>.*)\[.*");
        Regex genericClassRegex = new Regex(@"^(?<namespace>.*)\.(?<class>.*)\.<.*\.(?<method>.*)\(.*");

        var regexs = new[]
        {
            genericMethodRegex,
            genericClassRegex,
            noGenericsRegex,
        };

        Match match = null;
        foreach (var regex in regexs)
        {
            match = regex.Match(request.StackFrame);
            if (match.Success)
            {
                break;
            }
        }
        if (match != null)
        {
            var findMethodByNameRequest = new FindMethodByNameRequest
            {
                NamespaceName = match.Groups["namespace"].Value,
                TypeName = match.Groups["class"].Value,
                MethodName = match.Groups["method"].Value,
            };
            var handle = _findMethodByNameHandler.Handle(findMethodByNameRequest);
            var responsePacket = await handle;
            var result = responsePacket;

            return result;
        }

        return new ResponsePacket<LocationsResponse>
        {
            Body = new LocationsResponse()
        };
    }
}