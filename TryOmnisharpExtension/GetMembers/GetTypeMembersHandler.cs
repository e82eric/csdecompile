using System.Composition;
using System.Threading.Tasks;
using OmniSharp.Mef;

namespace TryOmnisharpExtension.GetMembers;

[OmniSharpHandler(Endpoints.GetTypeMembers, Languages.Csharp), Shared]
public class GetTypeMembersHandler : IRequestHandler<GetTypeMembersRequest, GetTypeMembersResponse>
{
    private readonly IlSpyGetMembersCommandFactory _commandFactory;

    [ImportingConstructor]
    public GetTypeMembersHandler(IlSpyGetMembersCommandFactory commandFactory)
    {
        _commandFactory = commandFactory;
    }
    
    public async Task<GetTypeMembersResponse> Handle(GetTypeMembersRequest request)
    {
        var command = _commandFactory.Find(request);
        var result = await command.Execute();
        return result;
    }
}