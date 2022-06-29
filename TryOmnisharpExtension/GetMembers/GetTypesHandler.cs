using System.Threading.Tasks;
using TryOmnisharpExtension.FindImplementations;

namespace TryOmnisharpExtension.GetMembers;

public class GetTypesHandler : HandlerBase<GetTypesRequest, FindImplementationsResponse>
{
    private readonly AllTypesRepository _typesRepository;

    public GetTypesHandler(AllTypesRepository typesRepository)
    {
        _typesRepository = typesRepository;
    }
    
    public override Task<FindImplementationsResponse> Handle(GetTypesRequest request)
    {
        var types = _typesRepository.GetAllTypes(request.SearchString);
        var response = new FindImplementationsResponse();
        foreach (var type in types)
        {
            response.Implementations.Add(type);
        }
        return Task.FromResult(response);
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