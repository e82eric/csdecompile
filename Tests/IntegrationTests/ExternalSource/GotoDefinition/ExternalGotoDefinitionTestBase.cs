﻿using NUnit.Framework;
using TryOmnisharpExtension;
using TryOmnisharpExtension.GotoDefinition;

namespace IntegrationTests;

public class ExternalGotoDefinitionTestBase : ExternalTestBase
{
    protected void SendRequestAndAssertLine(string filePath, int column, int line, string expected)
    {
        SendRequestAndAssertLine(
            Endpoints.DecompileGotoDefinition,
            filePath,
            column,
            line,
            expected);
    }

    private void SendRequestAndAssertLine(string command, string filePath, int column, int line, string expected)
    {
        var decompiledLocationRequest = new DecompiledLocationRequest
        {
            FileName = filePath,
            Column = column,
            IsDecompiled = false,
            Line = line
        };
        
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = command,
            Arguments = decompiledLocationRequest,
        };
        
        RequestAndCompare(
            request,
            expected);
    }
    
    protected void SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
        string filePath,
        int column,
        int line,
        string lineToFind,
        string tokenToRequest,
        string expected)
    {
        var requestArguments = GotoDefintionAndCreateRequestForToken(
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

        RequestAndCompare(request, expected);
    }
    
    private void RequestAndCompare(CommandPacket<DecompiledLocationRequest> request, string expected)
    {
        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, DecompileGotoDefinitionResponse>(request);

        Assert.True(response.Success);
        Assert.AreEqual(response.Body.Location.Type, ResponseLocationType.Decompiled);
        var decompileInfo = (DecompileInfo)response.Body.Location;

        var lines = GetLines(response.Body.SourceText);
        var sourceLine = lines[decompileInfo.Line - 1].Trim();
        Assert.AreEqual(expected, sourceLine);
    }
}