using OmniSharp.Mef;

namespace TryOmnisharpExtension;

[OmniSharpEndpoint(Endpoints.DecompileFindImplementations, typeof(DecompileFindImplementationsRequest), typeof(FindImplementationsResponse))]
public class DecompileFindImplementationsRequest : DecompiledLocationRequest
{
}