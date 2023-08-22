using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

public class ExternalGotoDefinitionTestBase : ExternalTestBase
{
    protected ExternalGotoDefinitionTestBase()
    {
    }

    protected ExternalGotoDefinitionTestBase(StdIoClient stdIoClient) : base(stdIoClient)
    {
    }

    protected void SendRequestAndAssertLocations(
        string filePath,
        int column,
        int line,
        IEnumerable<(LocationType type, string value, string shortTypeName)> expected)
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
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = decompiledLocationRequest,
        };

        var response =
            IoClient.ExecuteCommand<DecompiledLocationRequest, FindImplementationsResponse>(request);

        Assert.AreEqual(expected.Count(), response.Body.Implementations.Count());

        foreach (var expectedLocation in expected)
        {
            var found = response.Body.Implementations.Where(
                e => e.Type == expectedLocation.type &&
                     e.ContainingTypeShortName == expectedLocation.shortTypeName &&
                     e.SourceText == expectedLocation.value);

            Assert.NotNull(found);
        }
    }

    protected void SendRequestAndAssertLine(
        string filePath,
        int column,
        int line,
        string expected,
        string containingTypeFullName)
    {
        SendRequestAndAssertLine(
            Endpoints.DecompileGotoDefinition,
            filePath,
            column,
            line,
            expected,
            containingTypeFullName);
    }

    private void SendRequestAndAssertLine(
        string command,
        string filePath,
        int column,
        int line,
        string expected,
        string containingTypeFullName)
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
            Command = command,
            Arguments = decompiledLocationRequest,
        };
        
        RequestAndCompare(
            request,
            expected,
            containingTypeFullName);
    }
    
    protected void SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
        string filePath,
        int column,
        int line,
        string lineToFind,
        string tokenToRequest,
        string expected,
        string containingTypeFullName)
    {
        var requestArguments = GotoDefinitionAndCreateRequestForToken(
            filePath,
            column,
            line,
            lineToFind,
            tokenToRequest);
        
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = requestArguments,
        };

        RequestAndCompare(request, expected, containingTypeFullName);
    }
    
    protected void SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
        string filePath,
        int column,
        int line,
        string lineToFind,
        string tokenToRequest,
        string lineToFind2,
        string line2TokenRegex,
        string expected,
        string containingTypeFullName)
    {
        var requestArguments = GotoDefinitionAndCreateRequestForToken(
            filePath,
            column,
            line,
            lineToFind,
            tokenToRequest,
            lineToFind2,
            line2TokenRegex);
        
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = requestArguments,
        };

        RequestAndCompare(request, expected, containingTypeFullName);
    }
    protected void SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
        string filePath,
        int column,
        int line,
        string lineToFind,
        string tokenToRequest,
        string lineToFind2,
        string line2TokenRegex,
        string lineToFind3,
        string line3TokenRegex,
        string expected,
        string containingTypeFullName)
    {
        var requestArguments = GotoDefinitionAndCreateRequestForToken(
            filePath,
            column,
            line,
            lineToFind,
            tokenToRequest,
            lineToFind2,
            line2TokenRegex,
            lineToFind3,
            line3TokenRegex);
        
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = requestArguments,
        };

        RequestAndCompare(request, expected, containingTypeFullName);
    }
    
    private void RequestAndCompare(
        CommandPacket<DecompiledLocationRequest> request,
        string expected,
        string containingTypeFullName)
    {
        var sourceResponse = GotoDefinitionHelper.Run(IoClient, request);

        var lines = GetLines(sourceResponse.Body.SourceText);

        var sourceLine = lines[sourceResponse.Body.Location.Line - 1].Trim();
        Assert.AreEqual(expected, sourceLine);
        Assert.AreEqual(containingTypeFullName, sourceResponse.Body.Location.ContainingTypeFullName);
    }
}