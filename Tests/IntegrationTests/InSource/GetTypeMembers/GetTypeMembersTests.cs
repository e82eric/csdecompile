using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class GetTypeMembersTests
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "InSourceGetMembersTarget.cs");
    [Test]
    public void GetSimpleMembers()
    {
        RequestAndAssertCorrectLine(
            filePath:FilePath,
            column:1,
            line:2,
            new []
            {
                //I guess right now we are including the declaration in the response
                ("public InSourceGetMembersTarget()", "InSourceGetMembersTarget"),
                ("public string BasicProperty { get; set; }", "InSourceGetMembersTarget"),
                ("public void BasicMethod()", "InSourceGetMembersTarget"),
                ("public void BasicMethod(string param1)", "InSourceGetMembersTarget"),
            });
    }
    
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
    private static void AssertInSource(
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