using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace CsDecompileLib.Nuget;

public class GetNugetPackageVersionsHandler : HandlerBase<GetNugetPackageVersionsRequest, GetNugetPackageVersionsResponse>
{
    public override async Task<ResponsePacket<GetNugetPackageVersionsResponse>> Handle(GetNugetPackageVersionsRequest request)
    {
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;

        SourceCacheContext cache = new SourceCacheContext();
        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

        IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(
            request.PackageId,
            cache,
            logger,
            cancellationToken);

        var response = new GetNugetPackageVersionsResponse
        {
            PackageId = request.PackageId,
            ParentAssemblyMajorVersion = request.ParentAssemblyMajorVersion,
            ParentAssemblyMinorVersion = request.ParentAssemblyMinorVersion,
            ParentAssemblyBuildVersion = request.ParentAssemblyBuildVersion
        };
        foreach (NuGetVersion version in versions)
        {
            response.Packages.Add(new Package
            {
                PackageId = request.PackageId,
                PackageVersion = version.ToString(),
                MajorVersion = version.Version.Major,
                MinorVersion = version.Version.Minor,
                Build = version.Version.Build,
                Revision = version.Version.Revision,
                Patch = version.Patch
            });
        }

        return new ResponsePacket<GetNugetPackageVersionsResponse>
        {
            Body = response,
            Success = true
        };
    }
}