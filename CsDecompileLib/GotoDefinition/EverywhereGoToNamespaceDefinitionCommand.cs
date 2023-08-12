using System.Threading.Tasks;
using CsDecompileLib.GetMembers;

namespace CsDecompileLib.GotoDefinition;

public class EverywhereGoToNamespaceDefinitionCommand : INavigationCommand<FindImplementationsResponse>
{
    private readonly IlSpyTypesInReferencesSearcher _ilSpyTypesRepository;
    private readonly RoslynAllTypesRepository _roslynAllTypesRepository;
    private readonly string _namespaceString;

    public EverywhereGoToNamespaceDefinitionCommand(IlSpyTypesInReferencesSearcher ilSpyTypesRepository, RoslynAllTypesRepository roslynAllTypesRepository, string namespaceString)
    {
        _ilSpyTypesRepository = ilSpyTypesRepository;
        _namespaceString = namespaceString;
        _roslynAllTypesRepository = roslynAllTypesRepository;
    }

    public async Task<ResponsePacket<FindImplementationsResponse>> Execute()
    {
        var locations = await _ilSpyTypesRepository.GetAllTypes( info => info.NamespaceName == _namespaceString);
        var result = new FindImplementationsResponse();
        foreach (var location in locations)
        {
            result.Implementations.Add(location);
        }

        var roslynLocations = await _roslynAllTypesRepository.GetAllTypes(_namespaceString);
        foreach (var location in roslynLocations)
        {
            result.Implementations.Add(location);
        }

        return ResponsePacket.Ok(result);
    }
}