using OmniSharp.Mef;

namespace TryOmnisharpExtension.GotoDefinition
{
    [OmniSharpEndpoint(Endpoints.DecompileGotoDefinition, typeof(DecompileGotoDefinitionRequest), typeof(DecompileGotoDefinitionResponse))]
    public class DecompileGotoDefinitionRequest : DecompiledLocationRequest
    {
    }
}