using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace CsDecompileLib.Nuget;

public class
    AddNugetPackageAndDependenciesHandler : HandlerBase<AddNugetPackageRequest,
        AddPackageAndDependenciesResponse>
{
    private readonly NugetPackageDownloader _nugetPackageDownloader;

    public AddNugetPackageAndDependenciesHandler(NugetPackageDownloader nugetPackageDownloader)
    {
        _nugetPackageDownloader = nugetPackageDownloader;
    }

    public override async Task<ResponsePacket<AddPackageAndDependenciesResponse>> Handle(
        AddNugetPackageRequest request)
    {
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;

        var repositories = new List<SourceRepository>();
        foreach (var nugetSource in request.NugetSources)
        {
            var repository = Repository.Factory.GetCoreV3(nugetSource.Source);
            if (!string.IsNullOrEmpty(nugetSource.UserName))
            {
                repository.PackageSource.Credentials = new PackageSourceCredential(
                    nugetSource.Source,
                    nugetSource.UserName,
                    nugetSource.Password,
                    false,
                    "Basic");
            }

            repositories.Add(repository);
        }

        var packageDependencyInfos = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
        var packages = new HashSet<PackageIdentity>(PackageIdentityComparer.Default);

        SourceCacheContext cache = new SourceCacheContext();
        var packageIdentity = new PackageIdentity(request.PackageId, NuGetVersion.Parse(request.PackageVersion));

        var settings = Settings.LoadDefaultSettings(null);

        var nugetFramework = NuGetFramework.Parse(request.DependencyGroup);

        await GatherPackageDependencies(
            packageIdentity,
            packageDependencyInfos,
            packages,
            repositories,
            nugetFramework,
            cache,
            logger,
            cancellationToken);

        foreach (var package in packages)
        {
            await _nugetPackageDownloader.Download(
                request.RootPackageDirectory,
                package,
                nugetFramework,
                repositories,
                logger,
                cache,
                ClientPolicyContext.GetClientPolicy(settings, logger),
                cancellationToken);
        }

        return new ResponsePacket<AddPackageAndDependenciesResponse>
        {
            Success = true
        };
    }

    private async Task GatherPackageDependencies(
        PackageIdentity packageIdentity,
        HashSet<SourcePackageDependencyInfo> dependencies,
        HashSet<PackageIdentity> packages,
        IReadOnlyList<SourceRepository> repositories,
        NuGetFramework framework,
        SourceCacheContext cache,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (!packages.Contains(packageIdentity))
        {
            packages.Add(packageIdentity);
            foreach (var repository in repositories)
            {
                var dependencyInfoResource =
                    await repository.GetResourceAsync<DependencyInfoResource>(cancellationToken);
                var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                    packageIdentity,
                    framework,
                    cache,
                    logger,
                    cancellationToken);

                if (dependencyInfo != null)
                {
                    if (dependencies.Add(dependencyInfo))
                    {
                        foreach (var dependency in dependencyInfo.Dependencies)
                        {
                            await GatherPackageDependencies(
                                new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion),
                                dependencies,
                                packages,
                                repositories,
                                framework,
                                cache,
                                logger,
                                cancellationToken);
                        }
                    }
                }
            }
        }
    }
}