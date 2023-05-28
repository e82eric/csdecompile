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
        string tokenToRequest)
    {
        var result = GotoDefinitionAndCreateRequestForToken(
            filePath,
            column,
            line,
            targetLines =>
            {
                var lineText = targetLines.FirstOrDefault(l => l.Contains(lineToFind));
                var newLine = Array.IndexOf(targetLines, lineText) + 1;
                var newColumn = lineText.IndexOf(tokenToRequest) + 1;
                return (newLine, newColumn);
            });
        return result;
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
            delegate(string[] targetLines)
            {
                var lineText = targetLines.FirstOrDefault(l => l.Contains(lineToFind));
                var newLine = Array.IndexOf(targetLines, lineText) + 1;
                var newColumn = lineText.IndexOf(tokenToRequest) + 1;
                return (newLine, newColumn);
            });

        var result2 = GotoDefinitionAndCreateRequestForToken(
            Endpoints.DecompileGotoDefinition,
            result,
            targetLines =>
            {
                var lineText = targetLines.FirstOrDefault(l => l.Contains(lineToFind2));
                var newLine = Array.IndexOf(targetLines, lineText) + 1;
                var match = Regex.Match(lineText, line2TokenRegex);
                var newColumn = match.Index + 1;
                return (newLine, newColumn);
            });
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
            delegate(string[] targetLines)
            {
                var lineText = targetLines.FirstOrDefault(l => l.Contains(lineToFind));
                var newLine = Array.IndexOf(targetLines, lineText) + 1;
                var newColumn = lineText.IndexOf(tokenToRequest) + 1;
                return (newLine, newColumn);
            });

        var result2 = GotoDefinitionAndCreateRequestForToken(
            Endpoints.DecompileGotoDefinition,
            result,
            targetLines =>
            {
                var lineText = targetLines.FirstOrDefault(l => l.Contains(lineToFind2));
                var newLine = Array.IndexOf(targetLines, lineText) + 1;
                var match = Regex.Match(lineText, line2TokenRegex);
                var newColumn = match.Index + 2;
                return (newLine, newColumn);
            });
        
        var result3 = GotoDefinitionAndCreateRequestForToken(
            Endpoints.DecompileGotoDefinition,
            result2,
            targetLines =>
            {
                var lineText = targetLines.FirstOrDefault(l => l.Contains(lineToFind3));
                var newLine = Array.IndexOf(targetLines, lineText) + 1;
                var match = Regex.Match(lineText, line3TokenRegex);
                var newColumn = match.Index + 1;
                return (newLine, newColumn);
            });
        return result3;
    }
    
    protected DecompiledLocationRequest GotoDefinitionAndCreateRequestForToken(
        string filePath,
        int column,
        int line,
        Func<string[], (int line, int column)> findLineColumnInDecompiledSource) 
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
            findLineColumnInDecompiledSource);

        return result;
    }
    
    protected DecompiledLocationRequest GotoDefinitionAndCreateRequestForToken(
        string command,
        DecompiledLocationRequest requestArguments,
        Func<string[], (int line, int column)> findLineColumnInDecompiledSource)
    {
        var request = new CommandPacket<DecompiledLocationRequest>()
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
        var findInDecompiledSourceResult = findLineColumnInDecompiledSource(targetLines);

        var decompiledLocationRequest = new DecompiledLocationRequest
        {
            FileName = null,
            AssemblyFilePath = decompileInfo.AssemblyFilePath,
            ParentAssemblyFilePath = decompileInfo.ParentAssemblyFilePath,
            ContainingTypeFullName = decompileInfo.ContainingTypeFullName,
            Column = findInDecompiledSourceResult.column,
            Type = LocationType.Decompiled,
            Line = findInDecompiledSourceResult.line
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