using System.Threading.Tasks;
using CsDecompileLib.Roslyn;

namespace CsDecompileLib.Nuget;

public class SearchNugetForLocationHandler : HandlerBase<DecompiledLocationRequest, SearchNugetForLocationResponse>
{
    private readonly NugetSearcher _nugetSearcher;
    private readonly NavigationHandlerBase<DecompiledLocationRequest, SymbolInfo> _symbolInfoHandler;

    public SearchNugetForLocationHandler(
        NugetSearcher nugetSearcher,
        NavigationHandlerBase<DecompiledLocationRequest, SymbolInfo> symbolInfoHandler)
    {
        _nugetSearcher = nugetSearcher;
        _symbolInfoHandler = symbolInfoHandler;
    }

    public override async Task<ResponsePacket<SearchNugetForLocationResponse>> Handle(DecompiledLocationRequest request)
    {
        var symbolInfoResponse = await _symbolInfoHandler.Handle(request);
        var symbolInfo = symbolInfoResponse.Body;
        var response = new SearchNugetForLocationResponse();
        await _nugetSearcher.Search(symbolInfo.ParentAssemblyName, response);
        response.ParentAssemblyName = symbolInfo.ParentAssemblyName;
        response.ParentAssemblyMajorVersion = symbolInfo.ParentAssemblyMajorVersion;
        response.ParentAssemblyMinorVersion = symbolInfo.ParentAssemblyMinorVersion;
        response.ParentAssemblyBuildVersion = symbolInfo.ParentAssemblyBuildVersion;
        
        return new ResponsePacket<SearchNugetForLocationResponse>
        {
            Body = response,
            Success = true
        };
    }
}