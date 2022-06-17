using System.Composition;
using System.Threading.Tasks;
using OmniSharp.Mef;

namespace TryOmnisharpExtension.GetMembers;

[OmniSharpHandler(Endpoints.GetTypeMembers, Languages.Csharp), Shared]
public class GetTypeMembersHandler : IRequestHandler<GetTypeMembersRequest, GetTypeMembersResponse>
{
    private readonly IlSpyGetMembersCommandFactory _commandFactory;
    private readonly RoslynGetTypeMembersCommandFactory _roslynGetTypeMembersCommandFactory;

    [ImportingConstructor]
    public GetTypeMembersHandler(
        IlSpyGetMembersCommandFactory commandFactory,
        RoslynGetTypeMembersCommandFactory roslynGetTypeMembersCommandFactory)
    {
        _commandFactory = commandFactory;
        _roslynGetTypeMembersCommandFactory = roslynGetTypeMembersCommandFactory;
    }
    
    public async Task<GetTypeMembersResponse> Handle(GetTypeMembersRequest request)
    {
        INavigationCommand<GetTypeMembersResponse> command = null;
        
        if (!request.IsDecompiled)
        {
            command = await _roslynGetTypeMembersCommandFactory.Get(request);
        }
        else
        {
            command = _commandFactory.Find(request);
        }
        var result = await command.Execute();
        return result;
    }
}