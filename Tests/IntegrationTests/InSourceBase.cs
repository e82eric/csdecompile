using System.IO;
using NUnit.Framework;
using TryOmnisharpExtension;
using TryOmnisharpExtension.GotoDefinition;

namespace IntegrationTests;

public class InSourceBase
{
    protected void RequestAndAssertCorrectLine(string filePath, int column, int line, string expected)
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

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, DecompileGotoDefinitionResponse>(request);

        AssertInSource(response, expected);
    }

    private static void AssertInSource(ResponsePacket2<DecompileGotoDefinitionResponse> response, string expected)
    {
        Assert.True(response.Success);
        Assert.AreEqual(response.Body.Location.Type, ResponseLocationType.SourceCode);

        var location = (SourceFileInfo)response.Body.Location;

        var lines = File.ReadAllLines(location.FileName);
        var line = lines[location.Line - 1].Trim();

        Assert.AreEqual(expected, line);
    }
}