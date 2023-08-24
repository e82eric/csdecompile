using System.Threading.Tasks;

namespace CsDecompileLib.GetMembers;

public class GetTypesHandler : HandlerBase<GetTypesRequest, LocationsResponse>
{
    private readonly IlSpyTypesInReferencesSearcher _typesRepository;

    public GetTypesHandler(IlSpyTypesInReferencesSearcher typesRepository)
    {
        _typesRepository = typesRepository;
    }
    
    public override async Task<ResponsePacket<LocationsResponse>> Handle(GetTypesRequest request)
    {
        var types = await _typesRepository.GetAllTypes( info => info.ContainingTypeShortName.Contains(request.SearchString));

        var body = new LocationsResponse();
        foreach (var type in types)
        {
            body.Locations.Add(type);
        }

        var result = ResponsePacket.Ok(body);
        return result;
    }
}