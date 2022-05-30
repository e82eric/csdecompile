using System.Composition;
using System.Threading.Tasks;
using OmniSharp.Mef;
using TryOmnisharpExtension.GetMembers;

namespace TryOmnisharpExtension.ExternalAssemblies;

[OmniSharpHandler(Endpoints.SearchExternalAssembliesForType, Languages.Csharp), Shared]
public class SearchExternalAssembliesForTypeHandler : IRequestHandler<SearchExternalAssembliesForTypeRequest, SearchExternalAssembliesForTypeResponse>
{
    private readonly IlSpyAllTypesRepository _allTypesRepository;

    [ImportingConstructor]
    public SearchExternalAssembliesForTypeHandler(
        IlSpyAllTypesRepository allTypesRepository)
    {
        _allTypesRepository = allTypesRepository;
    }
    
    public async Task<SearchExternalAssembliesForTypeResponse> Handle(SearchExternalAssembliesForTypeRequest request)
    {
        var decompileInfos = await _allTypesRepository.GetAllTypes(request.TypeName);
        var response = new SearchExternalAssembliesForTypeResponse { FoundTypes = decompileInfos };
        return response;
    }
}