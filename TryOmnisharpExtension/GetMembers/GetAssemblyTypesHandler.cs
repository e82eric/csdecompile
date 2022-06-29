using System.Threading.Tasks;
using TryOmnisharpExtension.FindImplementations;

namespace TryOmnisharpExtension.GetMembers;

public class GetAssemblyTypesHandler : HandlerBase<GetAssemblyTypesRequest, FindImplementationsResponse>
{
    private readonly AllTypesRepository _typesRepository;

    public GetAssemblyTypesHandler(AllTypesRepository typesRepository)
    {
        _typesRepository = typesRepository;
    }
    
    public override Task<FindImplementationsResponse> Handle(GetAssemblyTypesRequest request)
    {
        var types = _typesRepository.GetAssemblyType(request.AssemblyFilePath);
        var response = new FindImplementationsResponse();
        foreach (var type in types)
        {
            response.Implementations.Add(type);
        }
        return Task.FromResult(response);
    }
}