using System.Collections.Generic;
using System.Linq;
using CsDecompileLib;
using CsDecompileLib.Roslyn;
using Newtonsoft.Json.Linq;
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

    protected void SendRequestAndAssertLine(
        string filePath,
        int column,
        int line,
        Dictionary<string, object> expected,
        Dictionary<string, string> expectedParameters)
    {
        var decompiledLocationRequest = new DecompiledLocationRequest
        {
            FileName = filePath,
            Column = column,
            Type = LocationType.SourceCode,
            Line = line
        };
        
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.SymbolInfo,
            Arguments = decompiledLocationRequest,
        };
        
        RequestAndCompare(
            request,
            expected,
            expectedParameters);
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

    private void RequestAndCompare(
        CommandPacket<DecompiledLocationRequest> request,
        Dictionary<string,object> expectedProperties,
        Dictionary<string, string> parameters)
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

        var actualParameters = response.Body.Properties["Parameters"] as JObject;
        Assert.AreEqual(parameters.Count, actualParameters.Count);
        foreach (var expectedParameter in parameters)
        {
            var matches = actualParameters.Properties().Where(
                p => p.Name == expectedParameter.Key && p.Value.ToString() == expectedParameter.Value);
            Assert.AreEqual(1, matches.Count());
        }
    }
}