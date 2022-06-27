using System.Threading.Tasks;

namespace TryOmnisharpExtension.GetMembers;

public class GetTypesHandler
{
    private readonly AllTypesRepository _typesRepository;

    public GetTypesHandler(AllTypesRepository typesRepository)
    {
        _typesRepository = typesRepository;
    }
    
    public Task<GetTypesResponse> Handle(GetTypesRequest request)
    {
        var types = _typesRepository.GetAllTypes(request.SearchString);
        var response = new GetTypesResponse {};
        foreach (var type in types)
        {
            response.Implementations.Add(type);
        }
        return Task.FromResult(response);
    }
}