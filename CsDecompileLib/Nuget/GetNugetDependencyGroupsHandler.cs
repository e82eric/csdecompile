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

public class GetNugetDependencyGroupsHandler : HandlerBase<AddNugetPackageAndDependenciesRequest, GetNugetDependencyGroupsResponse>
{
    public override async Task<ResponsePacket<GetNugetDependencyGroupsResponse>> Handle(
        AddNugetPackageAndDependenciesRequest request)
    {
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;
        SourceCacheContext cache = new SourceCacheContext();
        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        NuGetVersion packageVersion = new NuGetVersion(request.Version);
        FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

        var response = new GetNugetDependencyGroupsResponse();

        using (MemoryStream packageStream = new MemoryStream())
        {
            await resource.CopyNupkgToStreamAsync(
                request.Identity,
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
}