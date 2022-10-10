using CsDecompileLib;
using CsDecompileLib.GotoDefinition;

namespace IntegrationTests;

class DecompilerClient
{
    public ResponsePacket<GotoDefinitionResponse> GotoDefinition(DecompiledLocationRequest request)
    {
        var definitionRequest = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = request
        };

        var definitionResponse = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, GotoDefinitionResponse>(definitionRequest);

        return definitionResponse;
    }
}