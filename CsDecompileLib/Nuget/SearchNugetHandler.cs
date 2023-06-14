using System.Threading.Tasks;

namespace CsDecompileLib.Nuget;

public class SearchNugetHandler : HandlerBase<SearchNugetRequest, SearchNugetResponse>
{
    private readonly NugetSearcher _nugetSearcher;

    public SearchNugetHandler(NugetSearcher nugetSearcher)
    {
        _nugetSearcher = nugetSearcher;
    }
    public override async Task<ResponsePacket<SearchNugetResponse>> Handle(SearchNugetRequest request)
    {
        var response = new SearchNugetResponse();
        await _nugetSearcher.Search(request.SearchString, response);
        
        return new ResponsePacket<SearchNugetResponse>
        {
            Body = response,
            Success = true
        };
    }
}