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

public class AddNugetPackageAndDependenciesHandler : HandlerBase<AddNugetPackageAndDependenciesRequest, AddPackageAndDependenciesResponse>
{
    private readonly IDecompileWorkspace _decompileWorkspace;

    public AddNugetPackageAndDependenciesHandler(IDecompileWorkspace decompileWorkspace)
    {
        _decompileWorkspace = decompileWorkspace;
    }

    public override async Task<ResponsePacket<AddPackageAndDependenciesResponse>> Handle(AddNugetPackageAndDependenciesRequest request)
    {
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;
        var source = "https://api.nuget.org/v3/index.json";
        SourceRepository repository = Repository.Factory.GetCoreV3(source);
        var packages = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
        var response = new SearchNugetResponse();

        SourceCacheContext cache = new SourceCacheContext();
        var packageIdentity = new PackageIdentity(request.Identity, NuGetVersion.Parse(request.Version));

        var settings = Settings.LoadDefaultSettings(null);

        var nugetFramework = NuGetFramework.Parse(request.DependencyGroup);

        await GatherPackageDependencies(
            packageIdentity,
            packages,
            new[] { repository },
            nugetFramework,
            cache,
            logger,
            cancellationToken);

        foreach (var package in packages)
        {
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
            using MemoryStream packageStream = new MemoryStream();
            await resource.CopyNupkgToStreamAsync(
                package.Id,
                package.Version,
                packageStream,
                cache,
                logger,
                cancellationToken);

            packageStream.Seek(0, SeekOrigin.Begin);

            var downloadResult = await GlobalPackagesFolderUtility.AddPackageAsync(
                source,
                package,
                packageStream,
                request.RootPackageDirectory,
                parentId: Guid.Empty,
                ClientPolicyContext.GetClientPolicy(settings, logger),
                logger,
                cancellationToken);

            foreach (var group in await downloadResult.PackageReader.GetLibItemsAsync(cancellationToken))
            {
                if (DefaultCompatibilityProvider.Instance.IsCompatible(nugetFramework, group.TargetFramework))
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

        return new ResponsePacket<AddPackageAndDependenciesResponse>
        {
            Success = true
        };
    }

    private async Task GatherPackageDependencies(
        PackageIdentity packageIdentity,
        HashSet<SourcePackageDependencyInfo> dependencies,
        IReadOnlyList<SourceRepository> repositories,
        NuGetFramework framework,
        SourceCacheContext cache,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        foreach (var repository in repositories)
        {
            var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(cancellationToken);
            var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                packageIdentity,
                framework,
                cache,
                logger,
                cancellationToken);

            if (dependencies.Add(dependencyInfo))
            {
                foreach (var dependency in dependencyInfo.Dependencies)
                {
                    await GatherPackageDependencies(
                        new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion),
                        dependencies,
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