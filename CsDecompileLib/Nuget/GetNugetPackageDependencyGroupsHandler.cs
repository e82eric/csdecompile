using System;
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

public class
    GetNugetPackageDependencyGroupsHandler : HandlerBase<GetNugetPackageDependencyGroupsRequest,
        GetNugetDependencyGroupsResponse>
{
    public override async Task<ResponsePacket<GetNugetDependencyGroupsResponse>> Handle(
        GetNugetPackageDependencyGroupsRequest request)
    {
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;
        SourceCacheContext cache = new SourceCacheContext();

        foreach (var nugetSource in request.NugetSources)
        {
            SourceRepository repository = Repository.Factory.GetCoreV3(nugetSource);
            NuGetVersion packageVersion = new NuGetVersion(request.PackageVersion);
            FindPackageByIdResource resource =
                await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

            var response = new GetNugetDependencyGroupsResponse
            {
                NugetSources = request.NugetSources,
                PackageId = request.PackageId,
                PackageVersion = request.PackageVersion
            };

            using (MemoryStream packageStream = new MemoryStream())
            {
                await resource.CopyNupkgToStreamAsync(
                    request.PackageId,
                    packageVersion,
                    packageStream,
                    cache,
                    logger,
                    cancellationToken);

                using (PackageArchiveReader packageReader = new PackageArchiveReader(packageStream))
                {
                    NuspecReader nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);
                    var packageDependencyGroups = nuspecReader.GetDependencyGroups().ToList();
                    foreach (var dependencyGroup in packageDependencyGroups)
                    {
                        response.Groups.Add(dependencyGroup.TargetFramework.ToString());
                    }
                }
            }

            return new ResponsePacket<GetNugetDependencyGroupsResponse>
            {
                Body = response,
                Success = true
            };
        }

        throw new Exception("Package not found in sources");
    }
}