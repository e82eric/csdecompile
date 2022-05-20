using OmniSharp.Mef;

namespace TryOmnisharpExtension
{
    [OmniSharpEndpoint(Endpoints.DecompileGotoDefinition, typeof(DecompileGotoDefinitionRequest), typeof(DecompileGotoDefinitionResponse))]
    public class DecompileGotoDefinitionRequest : DecompiledLocationRequest
    {
    }
}