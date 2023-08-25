using CsDecompileLib;
using CsDecompileLib.GotoDefinition;

namespace IntegrationTests;

class DecompilerClient
{
    public ResponsePacket<DecompileAssemlbyResponse> GotoDefinition(DecompiledLocationRequest request)
    {
        var definitionRequest = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = request
        };

        var definitionResponse = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, DecompileAssemlbyResponse>(definitionRequest);

        return definitionResponse;
    }
}