using System.Threading.Tasks;

namespace CsDecompileLib.GetMembers;

public class GetAssembliesHandler : HandlerBase<object, GetAssembliesResponse>
{
    private readonly GetAssembliesCommandFactory _commandFactory;

    public GetAssembliesHandler(
        GetAssembliesCommandFactory commandFactory)
    {
        _commandFactory = commandFactory;
    }
    
    public override Task<ResponsePacket<GetAssembliesResponse>> Handle(object request)
    {
        var command = _commandFactory.Get();
        var body = command.Execute();
        var result = ResponsePacket.Ok(body);
        return Task.FromResult(result);
    }
}