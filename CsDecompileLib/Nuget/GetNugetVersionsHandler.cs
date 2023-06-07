using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace CsDecompileLib.Nuget;

public class GetNugetVersionsHandler : HandlerBase<GetNugetVersionsRequest, SearchNugetResponse>
{
    public override async Task<ResponsePacket<SearchNugetResponse>> Handle(GetNugetVersionsRequest request)
    {
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;

        SourceCacheContext cache = new SourceCacheContext();
        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

        IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(
            request.Identity,
            cache,
            logger,
            cancellationToken);

        var response = new SearchNugetResponse();
        foreach (NuGetVersion version in versions)
        {
            response.Packages.Add(new Package
            {
                Identity = request.Identity,
                Version = version.ToString(),
                MajorVersion = version.Version.Major,
                MinorVersion = version.Version.Minor,
                Build = version.Version.Build,
                Revision = version.Version.Revision,
                Patch = version.Patch
            });
        }

        return new ResponsePacket<SearchNugetResponse>
        {
            Body = response,
            Success = true
        };
    }
}