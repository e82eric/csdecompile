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

public class AddNugetPackageHandler : HandlerBase<AddNugetPackageRequest,
        AddPackageAndDependenciesResponse>
{
    private readonly NugetPackageDownloader _nugetPackageDownloader;

    public AddNugetPackageHandler(
        NugetPackageDownloader nugetPackageDownloader)
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

        SourceCacheContext cache = new SourceCacheContext();
        var packageIdentity = new PackageIdentity(request.PackageId, NuGetVersion.Parse(request.PackageVersion));

        var settings = Settings.LoadDefaultSettings(null);

        await _nugetPackageDownloader.Download(
            request.RootPackageDirectory,
            packageIdentity,
            repositories,
            logger,
            cache,
            ClientPolicyContext.GetClientPolicy(settings, logger),
            cancellationToken);

        return new ResponsePacket<AddPackageAndDependenciesResponse>
        {
            Success = true
        };
    }
}