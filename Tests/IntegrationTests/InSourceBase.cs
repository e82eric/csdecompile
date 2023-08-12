using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CsDecompileLib;
using CsDecompileLib.GotoDefinition;

namespace IntegrationTests;

public class InSourceBase : TestBase
{
    protected void RequestAndAssertCorrectLocations(
        string filePath,
        int column,
        int line,
        IEnumerable<(LocationType type, string value, string shortTypeName)> expected)
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
        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, FindImplementationsResponse>(request);
        Assert.True(response.Success);
        Assert.AreEqual(expected.Count(), response.Body.Implementations.Count());

        foreach (var expectedLocation in expected)
        {
            var foundLocation = response.Body.Implementations.Where(
                i => i.Type == expectedLocation.type &&
                     i.ContainingTypeShortName == expectedLocation.shortTypeName &&
                     i.SourceText == expectedLocation.value);
            Assert.NotNull(foundLocation);
        }
    }
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
        var response = ExecuteRequest<FindImplementationsResponse>(command, filePath, column, line);

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
        ResponsePacket<FindImplementationsResponse> response,
        string expected,
        string containingTypeFullName)
    {
        var implementations = response.Body.Implementations;
        Assert.AreEqual(1, implementations.Count);
        var location = implementations.First();
        Assert.AreEqual(location.Type, LocationType.SourceCode);
        AssertInSourceLocation(location, expected, containingTypeFullName);
    }
    
    private static void AssertInSourceLocation(ResponseLocation location, string expected, string containingTypeFullName)
    {
        var lines = InSourceGetLines(location);
        var line = lines[location.Line - 1].Trim();

        Assert.AreEqual(expected, line);
        Assert.AreEqual(containingTypeFullName, location.ContainingTypeFullName);
    }
}