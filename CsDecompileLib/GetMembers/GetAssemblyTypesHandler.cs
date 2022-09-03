using System.Threading.Tasks;
using CsDecompileLib.FindImplementations;

namespace CsDecompileLib.GetMembers;

public class GetAssemblyTypesHandler : HandlerBase<GetAssemblyTypesRequest, FindImplementationsResponse>
{
    private readonly AllTypesRepository _typesRepository;

    public GetAssemblyTypesHandler(AllTypesRepository typesRepository)
    {
        _typesRepository = typesRepository;
    }
    
    public override Task<ResponsePacket<FindImplementationsResponse>> Handle(GetAssemblyTypesRequest request)
    {
        var types = _typesRepository.GetAssemblyType(request.AssemblyFilePath);
        var body = new FindImplementationsResponse();
        foreach (var type in types)
        {
            body.Implementations.Add(type);
        }

        var result = ResponsePacket.Ok(body);
        
        return Task.FromResult(result);
    }
}