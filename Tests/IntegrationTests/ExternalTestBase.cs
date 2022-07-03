using System;
using System.Linq;
using NUnit.Framework;
using TryOmnisharpExtension;
using TryOmnisharpExtension.GotoDefinition;

namespace IntegrationTests;

public class ExternalTestBase
{
    protected void SendRequestAndAssertLine(string filePath, int column, int line, string expected)
    {
        var decompiledLocationRequest = new DecompiledLocationRequest
        {
            FileName = filePath,
            Column = column,
            IsDecompiled = false,
            Line = line
        };
        
        RequestAndCompare(decompiledLocationRequest, expected);
        // var request = new CommandPacket<DecompiledLocationRequest>
        // {
        //     Command = Endpoints.DecompileGotoDefinition,
        //     Arguments = decompiledLocationRequest
        // };
        //
        // var response = TestHarness.IoClient
        //     .ExecuteCommand<DecompiledLocationRequest, DecompileGotoDefinitionResponse>(request);
        //
        // Assert.True(response.Success);
        // Assert.AreEqual(response.Body.Location.Type, ResponseLocationType.Decompiled);
        // var decompileInfo = (DecompileInfo)response.Body.Location;
        //
        // string[] stringSeparators = { "\r\n" };
        // string[] lines = response.Body.SourceText.Split(stringSeparators, StringSplitOptions.None);
        // var sourceLine = lines[decompileInfo.Line - 1].Trim();
        // Assert.AreEqual(expected, sourceLine);
    }
    
    protected void SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
        string filePath,
        int column,
        int line,
        string lineToFind,
        string tokenToRequest,
        string expected)
    {
        var decompiledLocationRequest = GotoDefintionAndCreateRequestForToken(
            filePath,
            column,
            line,
            lineToFind,
            tokenToRequest);
        // var request = new CommandPacket<DecompiledLocationRequest>
        // {
        //     Command = Endpoints.DecompileGotoDefinition,
        //     Arguments = new DecompiledLocationRequest
        //     {
        //         FileName = filePath,
        //         Column = column,
        //         IsDecompiled = false,
        //         Line = line
        //     }
        // };
        //
        // var targetClasResponse = TestHarness.IoClient
        //     .ExecuteCommand<DecompiledLocationRequest, DecompileGotoDefinitionResponse>(request);
        //
        // Assert.True(targetClasResponse.Success);
        // Assert.AreEqual(targetClasResponse.Body.Location.Type, ResponseLocationType.Decompiled);
        // var decompileInfo = (DecompileInfo)targetClasResponse.Body.Location;
        //
        // string[] stringSeparators = { "\r\n" };
        // string[] targetLines = targetClasResponse.Body.SourceText.Split(stringSeparators, StringSplitOptions.None);
        // var lineText = targetLines.FirstOrDefault(l => l.Contains(lineToFind));
        // var newLine = Array.IndexOf(targetLines, lineText);
        // var newColumn = lineText.IndexOf(tokenToRequest);
        //
        // var decompiledLocationRequest = new DecompiledLocationRequest
        // {
        //     FileName = null,
        //     AssemblyFilePath = decompileInfo.AssemblyFilePath,
        //     ContainingTypeFullName = decompileInfo.ContainingTypeFullName,
        //     Column = newColumn + 1,
        //     IsDecompiled = true,
        //     Line = newLine + 1
        // };
        
        // var request2 = new CommandPacket<DecompiledLocationRequest>
        // {
        //     Command = Endpoints.DecompileGotoDefinition,
        //     Arguments = decompiledLocationRequest
        // };
        
        RequestAndCompare(decompiledLocationRequest, expected);
        
        // var responsePacket2 = TestHarness.IoClient
        //     .ExecuteCommand<DecompiledLocationRequest, DecompileGotoDefinitionResponse>(request2);
        //
        // string[] lines = responsePacket2.Body.SourceText.Split(stringSeparators, StringSplitOptions.None);
        // var newDecompileInfo = (DecompileInfo)responsePacket2.Body.Location;
        // var sourceLine = lines[newDecompileInfo.Line - 1].Trim();
        // Assert.AreEqual(expected, sourceLine);
    }

    private void RequestAndCompare(DecompiledLocationRequest requestArguments, string expected)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = requestArguments,
        };
        
        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, DecompileGotoDefinitionResponse>(request);

        Assert.True(response.Success);
        Assert.AreEqual(response.Body.Location.Type, ResponseLocationType.Decompiled);
        var decompileInfo = (DecompileInfo)response.Body.Location;

        string[] stringSeparators = { "\r\n" };
        string[] lines = response.Body.SourceText.Split(stringSeparators, StringSplitOptions.None);
        var sourceLine = lines[decompileInfo.Line - 1].Trim();
        Assert.AreEqual(expected, sourceLine);
    }

    protected DecompiledLocationRequest GotoDefintionAndCreateRequestForToken(
        string filePath,
        int column,
        int line,
        string lineToFind,
        string tokenToRequest)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = new DecompiledLocationRequest
            {
                FileName = filePath,
                Column = column,
                IsDecompiled = false,
                Line = line
            }
        };

        var targetClasResponse = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, DecompileGotoDefinitionResponse>(request);

        Assert.True(targetClasResponse.Success);
        Assert.AreEqual(targetClasResponse.Body.Location.Type, ResponseLocationType.Decompiled);
        var decompileInfo = (DecompileInfo)targetClasResponse.Body.Location;

        string[] stringSeparators = { "\r\n" };
        string[] targetLines = targetClasResponse.Body.SourceText.Split(stringSeparators, StringSplitOptions.None);
        var lineText = targetLines.FirstOrDefault(l => l.Contains(lineToFind));
        var newLine = Array.IndexOf(targetLines, lineText);
        var newColumn = lineText.IndexOf(tokenToRequest);

        var decompiledLocationRequest = new DecompiledLocationRequest
        {
            FileName = null,
            AssemblyFilePath = decompileInfo.AssemblyFilePath,
            ContainingTypeFullName = decompileInfo.ContainingTypeFullName,
            Column = newColumn + 1,
            IsDecompiled = true,
            Line = newLine + 1
        };
        return decompiledLocationRequest;
    }
}