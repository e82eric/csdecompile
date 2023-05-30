using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using CsDecompileLib;
using CsDecompileLib.GotoDefinition;

namespace IntegrationTests;

public class ExternalTestBase : TestBase
{
    protected StdIoClient IoClient { get; }

    protected ExternalTestBase()
    {
        IoClient = TestHarness.IoClient;
    }

    protected ExternalTestBase(StdIoClient stdIoClient)
    {
        IoClient = stdIoClient;
    }

    protected DecompiledLocationRequest GotoDefinitionAndCreateRequestForToken(
        string filePath,
        int column,
        int line,
        string lineToFind,
        string tokenToRequest,
        string lineToFind2,
        string line2TokenRegex)
    {
        var result = GotoDefinitionAndCreateRequestForToken(
            filePath,
            column,
            line,
            lineToFind,
            tokenToRequest);

        var result2 = GotoDefinitionAndCreateRequestForToken(
            Endpoints.DecompileGotoDefinition,
            result,
            lineToFind2,
            line2TokenRegex);
        return result2;
    }
    
    protected DecompiledLocationRequest GotoDefinitionAndCreateRequestForToken(
        string filePath,
        int column,
        int line,
        string lineToFind,
        string tokenToRequest,
        string lineToFind2,
        string line2TokenRegex,
        string lineToFind3,
        string line3TokenRegex)
    {
        var result = GotoDefinitionAndCreateRequestForToken(
            filePath,
            column,
            line,
            lineToFind,
            tokenToRequest);

        var result2 = GotoDefinitionAndCreateRequestForToken(
            Endpoints.DecompileGotoDefinition,
            result,
            lineToFind2,
            line2TokenRegex);

        var result3 = GotoDefinitionAndCreateRequestForToken(
            Endpoints.DecompileGotoDefinition,
            result2,
            lineToFind3,
            line3TokenRegex);
        return result3;
    }
    
    protected DecompiledLocationRequest GotoDefinitionAndCreateRequestForToken(
        string filePath,
        int column,
        int line,
        string lineToFind,
        string tokenRegex)
    {
        var decompiledLocationRequest = new DecompiledLocationRequest
        {
            FileName = filePath,
            Column = column,
            Type = LocationType.SourceCode,
            Line = line
        };

        var result = GotoDefinitionAndCreateRequestForToken(
            Endpoints.DecompileGotoDefinition,
            decompiledLocationRequest,
            lineToFind,
            tokenRegex);

        return result;
    }
    
    protected DecompiledLocationRequest GotoDefinitionAndCreateRequestForToken(
        string command,
        DecompiledLocationRequest requestArguments,
        string lineToFind,
        string tokenRegex)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = command,
            Arguments = requestArguments
        };
        var targetClasResponse = IoClient
            .ExecuteCommand<DecompiledLocationRequest, GotoDefinitionResponse>(request);

        Assert.True(targetClasResponse.Success);
        Assert.AreEqual(targetClasResponse.Body.Location.Type, LocationType.Decompiled);
        var decompileInfo = (DecompileInfo)targetClasResponse.Body.Location;

        var targetLines = GetLines(targetClasResponse.Body.SourceText);
        
        var lineText = targetLines.FirstOrDefault(line =>
        {
            var match = Regex.Match(line, lineToFind);
            return match.Success;
        });
        
        var newLine = Array.IndexOf(targetLines, lineText) + 1;
        var match = Regex.Match(lineText, tokenRegex);
        var group = match.Groups["token"];
        var newColumn = group.Index + 1;
        
        var decompiledLocationRequest = new DecompiledLocationRequest
        {
            FileName = null,
            AssemblyFilePath = decompileInfo.AssemblyFilePath,
            ParentAssemblyFilePath = decompileInfo.ParentAssemblyFilePath,
            ContainingTypeFullName = decompileInfo.ContainingTypeFullName,
            Column = newColumn,
            Type = LocationType.Decompiled,
            Line = newLine
        };
        return decompiledLocationRequest;
    }
    
    public static string[] GetLines(string sourceText)
    {
        string[] stringSeparators = { "\r\n" };
        var lines = sourceText.Split(stringSeparators, StringSplitOptions.None);
        return lines;
    }
}