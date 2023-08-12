using System.Threading.Tasks;

namespace CsDecompileLib.GetMembers;

public class GetTypesHandler : HandlerBase<GetTypesRequest, FindImplementationsResponse>
{
    private readonly IlSpyTypesInReferencesSearcher _typesRepository;

    public GetTypesHandler(IlSpyTypesInReferencesSearcher typesRepository)
    {
        _typesRepository = typesRepository;
    }
    
    public override async Task<ResponsePacket<FindImplementationsResponse>> Handle(GetTypesRequest request)
    {
        var types = await _typesRepository.GetAllTypes( info => info.ContainingTypeShortName.Contains(request.SearchString));

        var body = new FindImplementationsResponse();
        foreach (var type in types)
        {
            body.Implementations.Add(type);
        }

        var result = ResponsePacket.Ok(body);
        return result;
    }
}