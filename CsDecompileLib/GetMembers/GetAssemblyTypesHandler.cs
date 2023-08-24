using System.Threading.Tasks;

namespace CsDecompileLib.GetMembers;

public class GetAssemblyTypesHandler : HandlerBase<GetAssemblyTypesRequest, LocationsResponse>
{
    private readonly AllTypesRepository _typesRepository;

    public GetAssemblyTypesHandler(AllTypesRepository typesRepository)
    {
        _typesRepository = typesRepository;
    }

    public override Task<ResponsePacket<LocationsResponse>> Handle(GetAssemblyTypesRequest request)
    {
        var types = _typesRepository.GetAssemblyType(request.AssemblyFilePath);
        var body = new LocationsResponse();
        foreach (var type in types)
        {
            body.Locations.Add(type);
        }

        var result = ResponsePacket.Ok(body);

        return Task.FromResult(result);
    }
}