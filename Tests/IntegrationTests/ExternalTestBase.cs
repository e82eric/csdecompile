using System;
using System.Linq;
using NUnit.Framework;
using CsDecompileLib;
using CsDecompileLib.GotoDefinition;

namespace IntegrationTests;

public class ExternalTestBase : TestBase
{
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
            delegate(string[] targetLines)
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
        Func<string[], (int line, int column)> findLineColumnInDecompiledSource) 
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = new DecompiledLocationRequest
            {
                FileName = filePath,
                Column = column,
                Type = LocationType.SourceCode,
                Line = line
            }
        };

        var targetClasResponse = TestHarness.IoClient
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