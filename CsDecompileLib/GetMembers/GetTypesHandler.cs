﻿using System.Threading.Tasks;
using CsDecompileLib.FindImplementations;

namespace CsDecompileLib.GetMembers;

public class GetTypesHandler : HandlerBase<GetTypesRequest, FindImplementationsResponse>
{
    private readonly AllTypesRepository _typesRepository;

    public GetTypesHandler(AllTypesRepository typesRepository)
    {
        _typesRepository = typesRepository;
    }
    
    public override Task<ResponsePacket<FindImplementationsResponse>> Handle(GetTypesRequest request)
    {
        var types = _typesRepository.GetAllTypes(request.SearchString);
        var body = new FindImplementationsResponse();
        foreach (var type in types)
        {
            body.Implementations.Add(type);
        }

        var result = ResponsePacket.Ok(body);
        return Task.FromResult(result);
    }

    public FindImplementationsResponse HandleGetAssemblyTypes(GetAssemblyTypesRequest request)
    {
        var types = _typesRepository.GetAssemblyType(request.AssemblyFilePath);
        var response = new FindImplementationsResponse();
        foreach (var type in types)
        {
            response.Implementations.Add(type);
        }
        return response;
    }
}