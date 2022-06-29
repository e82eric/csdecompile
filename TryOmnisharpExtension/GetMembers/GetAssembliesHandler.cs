using System.Threading.Tasks;

namespace TryOmnisharpExtension.GetMembers;

public class GetAssembliesHandler : HandlerBase<object, GetAssembliesResponse>
{
    private readonly GetAssembliesCommandFactory _commandFactory;

    public GetAssembliesHandler(
        GetAssembliesCommandFactory commandFactory)
    {
        _commandFactory = commandFactory;
    }
    
    public override Task<GetAssembliesResponse> Handle(object request)
    {
        var command = _commandFactory.Get();
        var result = command.Execute();
        return Task.FromResult(result);
    }
}