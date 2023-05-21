using System.Collections.Generic;
using CsDecompileLib;
using CsDecompileLib.GotoDefinition;
using CsDecompileLib.Roslyn;
using NUnit.Framework;

namespace IntegrationTests;

public class ExternalGetSymbolInfoTestBase : ExternalTestBase
{
    protected void SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
        string filePath,
        int column,
        int line,
        string lineToFind,
        string tokenToRequest,
        Dictionary<string,object> expected)
    {
        var requestArguments = GotoDefinitionAndCreateRequestForToken(
            filePath,
            column,
            line,
            lineToFind,
            tokenToRequest);
        
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.SymbolInfo,
            Arguments = requestArguments,
        };

        RequestAndCompare(request, expected);
    }
    
    private void RequestAndCompare(CommandPacket<DecompiledLocationRequest> request, Dictionary<string,object> expectedProperties)
    {
        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, SymbolInfo>(request);

        Assert.True(response.Success);

        foreach (var key in expectedProperties.Keys)
        {
            var expected = expectedProperties[key];
            var actual = response.Body.Properties[key];
            Assert.AreEqual(expected, actual);
        }
    }
}