using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace CsDecompileLib.Nuget;

public class NugetSearcher
{
    public async Task Search(string searchString, NugetSource[] nugetSources, SearchNugetResponse response)
    {
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;
        response.SearchString = searchString;
        response.NugetSources = nugetSources;
        SearchFilter searchFilter = new SearchFilter(includePrerelease: true);
        var itemsPerPage = 100;
        var offset = 0;

        foreach (var nugetSource in nugetSources)
        {
            SourceRepository repository = Repository.Factory.GetCoreV3(nugetSource.Source);
            if (!string.IsNullOrEmpty(nugetSource.UserName))
            {
                repository.PackageSource.Credentials = new PackageSourceCredential(
                    nugetSource.Source,
                    nugetSource.UserName,
                    nugetSource.Password,
                    false,
                    "Basic");
            }

            PackageSearchResource resource =
                await repository.GetResourceAsync<PackageSearchResource>(cancellationToken);
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
}