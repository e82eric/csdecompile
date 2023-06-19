using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
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
    AddNugetPackageAndDependenciesHandler : HandlerBase<AddNugetPackageAndDependenciesRequest,
        AddPackageAndDependenciesResponse>
{
    private readonly IDecompileWorkspace _decompileWorkspace;

    public AddNugetPackageAndDependenciesHandler(IDecompileWorkspace decompileWorkspace)
    {
        _decompileWorkspace = decompileWorkspace;
    }

    public override async Task<ResponsePacket<AddPackageAndDependenciesResponse>> Handle(
        AddNugetPackageAndDependenciesRequest request)
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
            foreach (var repository in repositories)
            {
                var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
                using MemoryStream packageStream = new MemoryStream();
                if (resource != null)
                {
                    if (await resource.CopyNupkgToStreamAsync(
                            package.Id,
                            package.Version,
                            packageStream,
                            cache,
                            logger,
                            cancellationToken))
                    {
                        await DownloadPackageAndDependencies(request, packageStream, repository, package, settings, logger, cancellationToken, nugetFramework);
                        break;
                    }

                    if (await resource.CopyNupkgToStreamAsync(
                            package.Id,
                            new NuGetVersion(package.Version.Major, package.Version.Minor, package.Version.Patch),
                            packageStream,
                            cache,
                            logger,
                            cancellationToken))
                    {
                        await DownloadPackageAndDependencies(request, packageStream, repository, package, settings, logger, cancellationToken, nugetFramework);
                        break;
                    }
                }
            }
        }

        return new ResponsePacket<AddPackageAndDependenciesResponse>
        {
            Success = true
        };
    }

    private async Task DownloadPackageAndDependencies(AddNugetPackageAndDependenciesRequest request,
        MemoryStream packageStream, SourceRepository repository, PackageIdentity package, ISettings settings,
        ILogger logger, CancellationToken cancellationToken, NuGetFramework nugetFramework)
    {
        packageStream.Seek(0, SeekOrigin.Begin);

        var downloadResult = await GlobalPackagesFolderUtility.AddPackageAsync(
            repository.PackageSource.Source,
            package,
            packageStream,
            request.RootPackageDirectory,
            parentId: Guid.Empty,
            ClientPolicyContext.GetClientPolicy(settings, logger),
            logger,
            cancellationToken);

        foreach (var group in await downloadResult.PackageReader.GetLibItemsAsync(cancellationToken))
        {
            if (DefaultCompatibilityProvider.Instance.IsCompatible(nugetFramework,
                    group.TargetFramework))
            {
                foreach (var item in group.Items)
                {
                    var fi = new FileInfo(
                        $"{request.RootPackageDirectory}\\{package.Id}\\{package.Version}\\{item}");
                    _decompileWorkspace.LoadDllsInDirectory(fi.Directory);
                }
            }
        }
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