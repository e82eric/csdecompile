using System.Threading.Tasks;

namespace TryOmnisharpExtension.GetMembers;

public class GetTypesHandler : HandlerBase<GetTypesRequest, GetTypesResponse>
{
    private readonly AllTypesRepository _typesRepository;

    public GetTypesHandler(AllTypesRepository typesRepository)
    {
        _typesRepository = typesRepository;
    }
    
    public override Task<GetTypesResponse> Handle(GetTypesRequest request)
    {
        var types = _typesRepository.GetAllTypes(request.SearchString);
        var response = new GetTypesResponse();
        foreach (var type in types)
        {
            response.Implementations.Add(type);
        }
        return Task.FromResult(response);
    }

    public GetTypesResponse HandleGetAssemblyTypes(GetAssemblyTypesRequest request)
    {
        var types = _typesRepository.GetAssemblyType(request.AssemblyFilePath);
        var response = new GetTypesResponse();
        foreach (var type in types)
        {
            response.Implementations.Add(type);
        }
        return response;
    }
}