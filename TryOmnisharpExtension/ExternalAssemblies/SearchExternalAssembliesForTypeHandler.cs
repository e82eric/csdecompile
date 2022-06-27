using System.Threading.Tasks;
using TryOmnisharpExtension.GetMembers;

namespace TryOmnisharpExtension.ExternalAssemblies;

public class SearchExternalAssembliesForTypeHandler
{
    private readonly IlSpyAllTypesRepository _allTypesRepository;

    public SearchExternalAssembliesForTypeHandler(
        IlSpyAllTypesRepository allTypesRepository)
    {
        _allTypesRepository = allTypesRepository;
    }
    
    public Task<SearchExternalAssembliesForTypeResponse> Handle(SearchExternalAssembliesForTypeRequest request)
    {
        var decompileInfos = _allTypesRepository.GetAllTypes(request.TypeName);
        var response = new SearchExternalAssembliesForTypeResponse { FoundTypes = decompileInfos };
        return Task.FromResult(response);
    }
}