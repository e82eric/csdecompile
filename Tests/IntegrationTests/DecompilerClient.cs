using TryOmnisharpExtension;
using TryOmnisharpExtension.GotoDefinition;

namespace IntegrationTests;

class DecompilerClient
{
    public ResponsePacket<DecompileGotoDefinitionResponse> GotoDefinition(DecompiledLocationRequest request)
    {
        var definitionRequest = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = request
        };

        var definitionResponse = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, DecompileGotoDefinitionResponse>(definitionRequest);

        return definitionResponse;
    }
}