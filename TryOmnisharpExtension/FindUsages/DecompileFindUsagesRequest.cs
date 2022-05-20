using OmniSharp.Mef;
using OmniSharp.Models.FindUsages;
using TryOmnisharpExtension.FindUsages;

namespace TryOmnisharpExtension;

[OmniSharpEndpoint(Endpoints.DecompileFindUsages, typeof(DecompileFindUsagesRequest), typeof(FindUsagesResponse))]
public class DecompileFindUsagesRequest : DecompiledLocationRequest
{
}
