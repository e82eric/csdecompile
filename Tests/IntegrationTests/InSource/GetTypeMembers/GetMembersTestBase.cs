using System.Collections.Generic;
using System.Linq;
using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

public class GetMembersTestBase
{
    protected void RequestAndAssertCorrectLine(
        string filePath,
        int column,
        int line,
        IEnumerable<(string text, string shortName)> expected)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.GetTypeMembers,
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

        AssertInSource(response, expected);
    }

    private void AssertInSource(
        ResponsePacket<FindImplementationsResponse> response,
        IEnumerable<(string text, string shortName)> expected)
    {
        Assert.True(response.Success);
        Assert.AreEqual(response.Body.Implementations.Count, expected.Count());

        foreach (var implementation in response.Body.Implementations)
        {
            Assert.AreEqual(implementation.Type, LocationType.SourceCode);

            var fromExpected = expected.FirstOrDefault(e => e.text == implementation.SourceText);
            Assert.NotNull(fromExpected);
            Assert.AreEqual(fromExpected.shortName, implementation.ContainingTypeShortName);
        }
    }
}