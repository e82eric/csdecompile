using OmniSharp.Mef;

namespace TryOmnisharpExtension.FindUsages;

[OmniSharpEndpoint(Endpoints.DecompileFindUsages, typeof(DecompileFindUsagesRequest), typeof(FindUsagesResponse))]
public class DecompileFindUsagesRequest : DecompiledLocationRequest
{
}
