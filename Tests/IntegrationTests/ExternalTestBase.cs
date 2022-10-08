using System;
using System.Linq;
using NUnit.Framework;
using CsDecompileLib;
using CsDecompileLib.GotoDefinition;

namespace IntegrationTests;

public class ExternalTestBase : TestBase
{
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
                Type = LocationType.SourceCode,
                Line = line
            }
        };

        var targetClasResponse = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, DecompileGotoDefinitionResponse>(request);

        Assert.True(targetClasResponse.Success);
        Assert.AreEqual(targetClasResponse.Body.Location.Type, LocationType.Decompiled);
        var decompileInfo = (DecompileInfo)targetClasResponse.Body.Location;

        var targetLines = GetLines(targetClasResponse.Body.SourceText);
        var lineText = targetLines.FirstOrDefault(l => l.Contains(lineToFind));
        var newLine = Array.IndexOf(targetLines, lineText);
        var newColumn = lineText.IndexOf(tokenToRequest);

        var decompiledLocationRequest = new DecompiledLocationRequest
        {
            FileName = null,
            AssemblyFilePath = decompileInfo.AssemblyFilePath,
            ContainingTypeFullName = decompileInfo.ContainingTypeFullName,
            Column = newColumn + 1,
            Type = LocationType.Decompiled,
            Line = newLine + 1
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