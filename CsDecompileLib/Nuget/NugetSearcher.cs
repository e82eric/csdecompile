using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace CsDecompileLib.Nuget;

public class NugetSearcher
{
    public async Task Search(string searchString, SearchNugetResponse response)
    {
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;

        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        PackageSearchResource resource = await repository.GetResourceAsync<PackageSearchResource>(cancellationToken);
        SearchFilter searchFilter = new SearchFilter(includePrerelease: true);

        var itemsPerPage = 100;
        var offset = 0;
        response.SearchString = searchString;
        bool anyResults = true;
        while (anyResults)
        {
            IEnumerable<IPackageSearchMetadata> results = await resource.SearchAsync(
                searchString,
                searchFilter,
                skip: offset,
                take: itemsPerPage,
                logger,
                cancellationToken);
        
            foreach (IPackageSearchMetadata result in results)
            {
                var package = new Package
                {
                    PackageId = result.Identity.Id,
                };
                response.Packages.Add(package);
            }

            offset += itemsPerPage;
            anyResults = results.Any();
        }
    }
}