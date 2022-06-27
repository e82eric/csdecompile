﻿using System.Threading.Tasks;

namespace TryOmnisharpExtension.GetMembers;

public class GetTypeMembersHandler
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