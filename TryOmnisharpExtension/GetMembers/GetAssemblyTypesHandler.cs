using System.Threading.Tasks;

namespace TryOmnisharpExtension.GetMembers;

public class GetAssemblyTypesHandler : HandlerBase<GetAssemblyTypesRequest, GetTypesResponse>
{
    private readonly AllTypesRepository _typesRepository;

    public GetAssemblyTypesHandler(AllTypesRepository typesRepository)
    {
        _typesRepository = typesRepository;
    }
    
    public override Task<GetTypesResponse> Handle(GetAssemblyTypesRequest request)
    {
        var types = _typesRepository.GetAssemblyType(request.AssemblyFilePath);
        var response = new GetTypesResponse();
        foreach (var type in types)
        {
            response.Implementations.Add(type);
        }
        return Task.FromResult(response);
    }
}