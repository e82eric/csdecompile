using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace CsDecompileLib.Nuget;

public class SearchNugetHandler : HandlerBase<SearchNugetRequest, SearchNugetResponse>
{
    public override async Task<ResponsePacket<SearchNugetResponse>> Handle(SearchNugetRequest request)
    {
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;

        SourceCacheContext cache = new SourceCacheContext();
        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        PackageSearchResource resource = await repository.GetResourceAsync<PackageSearchResource>();
        SearchFilter searchFilter = new SearchFilter(includePrerelease: true);

        var itemsPerPage = 100;
        var offset = 0;
        var response = new SearchNugetResponse();
        bool anyResults = true;
        while (anyResults)
        {
            IEnumerable<IPackageSearchMetadata> results = await resource.SearchAsync(
                request.SearchString,
                searchFilter,
                skip: offset,
                take: itemsPerPage,
                logger,
                cancellationToken);
        
            foreach (IPackageSearchMetadata result in results)
            {
                var package = new Package
                {
                    Identity = result.Identity.Id,
                };
                response.Packages.Add(package);
            }

            offset += itemsPerPage;
            anyResults = results.Any();
        }
        
        return new ResponsePacket<SearchNugetResponse>
        {
            Body = response,
            Success = true
        };
    }
}