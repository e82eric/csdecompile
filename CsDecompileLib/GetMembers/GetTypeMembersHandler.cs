using System.Threading.Tasks;
using CsDecompileLib.FindImplementations;

namespace CsDecompileLib.GetMembers;

public class GetTypeMembersHandler : HandlerBase<DecompiledLocationRequest, FindImplementationsResponse>
{
    private readonly IlSpyGetMembersCommandFactory _commandFactory;
    private readonly RoslynGetTypeMembersCommandFactory _roslynGetTypeMembersCommandFactory;

    public GetTypeMembersHandler(
        IlSpyGetMembersCommandFactory commandFactory,
        RoslynGetTypeMembersCommandFactory roslynGetTypeMembersCommandFactory)
    {
        _commandFactory = commandFactory;
        _roslynGetTypeMembersCommandFactory = roslynGetTypeMembersCommandFactory;
    }
    
    public override async Task<ResponsePacket<FindImplementationsResponse>> Handle(DecompiledLocationRequest request)
    {
        INavigationCommand<FindImplementationsResponse> command = null;
        
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