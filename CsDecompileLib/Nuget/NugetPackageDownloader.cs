using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace CsDecompileLib.Nuget;

public class NugetPackageDownloader
{
    private readonly IDecompileWorkspace _workspace;

    public NugetPackageDownloader(IDecompileWorkspace workspace)
    {
        _workspace = workspace;
    }
    public async Task Download(
        string rootPackageDirectory,
        PackageIdentity packageIdentity,
        IReadOnlyList<SourceRepository> repositories,
        ILogger logger,
        SourceCacheContext cache,
        ClientPolicyContext clientPolicyContext,
        CancellationToken cancellationToken)
    {
        foreach (var repository in repositories)
        {
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
            using MemoryStream packageStream = new MemoryStream();
            if (resource != null)
            {
                if (await resource.CopyNupkgToStreamAsync(
                        packageIdentity.Id,
                        packageIdentity.Version,
                        packageStream,
                        cache,
                        logger,
                        cancellationToken))
                {
                    await DownloadPackageAndDependencies(rootPackageDirectory, packageStream, repository, packageIdentity, clientPolicyContext, logger,
                        cancellationToken);
                    break;
                }

                if (await resource.CopyNupkgToStreamAsync(
                        packageIdentity.Id,
                        new NuGetVersion(packageIdentity.Version.Major, packageIdentity.Version.Minor, packageIdentity.Version.Patch),
                        packageStream,
                        cache,
                        logger,
                        cancellationToken))
                {
                    await DownloadPackageAndDependencies(rootPackageDirectory, packageStream, repository, packageIdentity, clientPolicyContext, logger,
                        cancellationToken);
                    break;
                }
            }
        }
    }
    
    private async Task DownloadPackageAndDependencies(string rootPackageDirectory,
        MemoryStream packageStream, SourceRepository repository, PackageIdentity package, ClientPolicyContext clientPolicyContext,
        ILogger logger, CancellationToken cancellationToken)
    {
        packageStream.Seek(0, SeekOrigin.Begin);

        var downloadResult = await GlobalPackagesFolderUtility.AddPackageAsync(
            repository.PackageSource.Source,
            package,
            packageStream,
            rootPackageDirectory,
            parentId: Guid.Empty,
            clientPolicyContext,
            logger,
            cancellationToken);

        var directory = new FileInfo(((FileStream)downloadResult.PackageStream).Name).Directory;
        _workspace.LoadDllsInDirectory(directory);
    }
}