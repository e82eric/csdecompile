﻿using NUnit.Framework;
using CsDecompileLib;
using CsDecompileLib.GotoDefinition;

namespace IntegrationTests;

public class ExternalGotoDefinitionTestBase : ExternalTestBase
{
    public ExternalGotoDefinitionTestBase() : base()
    {
    }

    public ExternalGotoDefinitionTestBase(StdIoClient stdIoClient) : base(stdIoClient)
    {
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
        var response = IoClient
            .ExecuteCommand<DecompiledLocationRequest, GotoDefinitionResponse>(request);

        Assert.True(response.Success);
        Assert.AreEqual(response.Body.Location.Type, LocationType.Decompiled);
        var decompileInfo = (DecompileInfo)response.Body.Location;

        var lines = GetLines(response.Body.SourceText);
        var sourceLine = lines[decompileInfo.Line - 1].Trim();
        Assert.AreEqual(expected, sourceLine);
        Assert.AreEqual(containingTypeFullName, decompileInfo.ContainingTypeFullName);
    }
}