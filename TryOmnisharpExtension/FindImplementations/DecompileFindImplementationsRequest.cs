using OmniSharp.Mef;

namespace TryOmnisharpExtension.FindImplementations;

[OmniSharpEndpoint(Endpoints.DecompileFindImplementations, typeof(DecompileFindImplementationsRequest), typeof(FindImplementationsResponse))]
public class DecompileFindImplementationsRequest : DecompiledLocationRequest
{
}