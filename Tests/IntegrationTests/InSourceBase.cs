using NUnit.Framework;
using CsDecompileLib;
using CsDecompileLib.GotoDefinition;

namespace IntegrationTests;

public class InSourceBase : TestBase
{
    protected void RequestAndAssertCorrectLine(
        string filePath,
        int column,
        int line,
        string expected,
        string containingTypeFullName)
    {
        RequestAndAssertCorrectLine(
            Endpoints.DecompileGotoDefinition,
            filePath,
            column,
            line,
            expected,
            containingTypeFullName);
    }
    
    protected void RequestAndAssertCorrectLine(
        string command,
        string filePath,
        int column,
        int line,
        string expected,
        string containingTypeFullName)
    {
        var response = ExecuteRequest<GotoDefinitionResponse>(command, filePath, column, line);

        AssertInSource(response, expected, containingTypeFullName);
    }

    protected static ResponsePacket<T> ExecuteRequest<T>(string command, string filePath, int column, int line)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = command,
            Arguments = new DecompiledLocationRequest
            {
                FileName = filePath,
                Column = column,
                Type = LocationType.SourceCode,
                Line = line
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, T>(request);
        Assert.True(response.Success);
        return response;
    }

    private static void AssertInSource(
        ResponsePacket<GotoDefinitionResponse> response,
        string expected,
        string containingTypeFullName)
    {
        Assert.AreEqual(response.Body.Location.Type, LocationType.SourceCode);
        AssertInSourceLocation(response.Body.Location, expected, containingTypeFullName);
    }
    
    private static void AssertInSourceLocation(ResponseLocation location, string expected, string containingTypeFullName)
    {
        var lines = InSourceGetLines(location);
        var line = lines[location.Line - 1].Trim();

        Assert.AreEqual(expected, line);
        Assert.AreEqual(containingTypeFullName, location.ContainingTypeFullName);
    }
}