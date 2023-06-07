using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace CsDecompileLib.Nuget;

public class GetNugetDependenciesHandler : HandlerBase<GetNugetDependenciesRequest, SearchNugetResponse>
{
    public override async Task<ResponsePacket<SearchNugetResponse>> Handle(GetNugetDependenciesRequest request)
    {
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;

        var response = new SearchNugetResponse();

        await GetPackageDependencies(
            request.Identity,
            request.Version,
            request.DependencyGroup,
            logger,
            cancellationToken, response);

        return new ResponsePacket<SearchNugetResponse>
        {
            Body = response,
            Success = true
        };
    }

    private static async Task GetPackageDependencies(
        string identity,
        string version,
        string targetFramework,
        ILogger logger,
        CancellationToken cancellationToken,
        SearchNugetResponse response)
    {
        SourceCacheContext cache = new SourceCacheContext();
        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        NuGetVersion packageVersion = new NuGetVersion(version);
        FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

        var packagesToAdd = new List<Package>();
        using (MemoryStream packageStream = new MemoryStream())
        {
            await resource.CopyNupkgToStreamAsync(
                identity,
                packageVersion,
                packageStream,
                cache,
                logger,
                cancellationToken);

            using (PackageArchiveReader packageReader = new PackageArchiveReader(packageStream))
            {
                NuspecReader nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);
                var packageDependencyGroups = nuspecReader.GetDependencyGroups().ToList();
                var dependencyGroup = packageDependencyGroups.FirstOrDefault(
                    g => g.TargetFramework.ToString() == targetFramework);
                if (dependencyGroup != null)
                {
                    foreach (var packageDependency in dependencyGroup.Packages)
                    {
                        var package = new Package
                        {
                            Identity = packageDependency.Id,
                            Version = packageDependency.VersionRange.MinVersion.ToString()
                        };
                        packagesToAdd.Add(package);
                    }
                }
            }
        }

        foreach (var package in packagesToAdd)
        {
            if (!response.Packages.Any(p => p.Identity == package.Identity))
            {
                response.Packages.Add(package);
                await GetPackageDependencies(
                    package.Identity,
                    package.Version,
                    targetFramework,
                    logger,
                    cancellationToken,
                    response);
            }
        }
    }
}