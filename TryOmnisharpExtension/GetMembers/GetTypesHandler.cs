using System.Composition;
using System.Threading.Tasks;
using OmniSharp.Mef;

namespace TryOmnisharpExtension.IlSpy;

[OmniSharpHandler(Endpoints.GetTypes, Languages.Csharp), Shared]
public class GetTypesHandler : IRequestHandler<GetTypesRequest, GetTypesResponse>
{
    private readonly AllTypesRepository _typesRepository;

    [ImportingConstructor]
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