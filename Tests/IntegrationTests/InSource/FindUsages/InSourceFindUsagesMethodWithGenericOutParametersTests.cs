using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceFindUsagesMethodWithGenericOutParametersTests
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "InSourceFindUsagesMethodWithGenericOutParametersTarget.cs");
    [Test]
    public void Test1()
    {
        RequestAndAssertCorrectLine(
            filePath:FilePath,
            column:21,
            line:5,
            new []
            {
                //I guess right now we are including the declaration in the response
                ("a.TryRun(default, out _);",
                    "InSourceFindUsagesMethodWithGenericOutParametersUser",
                    "InSourceFindUsagesMethodWithGenericOutParametersUser"),
                ("b.TryRun(default, out _);",
                    "InSourceFindUsagesMethodWithGenericOutParametersUser",
                    "InSourceFindUsagesMethodWithGenericOutParametersUser"),
                ("public bool TryRun(T1 t1, out T2 t2)",
                    "InSourceFindUsagesMethodWithGenericOutParametersTarget",
                    "InSourceFindUsagesMethodWithGenericOutParametersTarget`2"),
            });
    }

    protected void RequestAndAssertCorrectLine(
        string filePath,
        int column,
        int line,
        IEnumerable<(string, string, string)> expected)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileFindUsages,
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
        IEnumerable<(string line, string shortName, string fullName)> expected)
    {
        Assert.True(response.Success);

        foreach (var implementation in response.Body.Implementations)
        {
            Assert.AreEqual(implementation.Type, LocationType.SourceCode);

            var location = (SourceFileInfo)implementation;

            var lines = File.ReadAllLines(location.FileName);
            var line = lines[location.Line - 1].Trim();

            var fromExpected = expected.Where(e =>
                e.line.Contains(line.Trim()) &&
                e.shortName == implementation.ContainingTypeShortName &&
                e.fullName == implementation.ContainingTypeFullName);
            Assert.AreEqual(1,fromExpected.Count());
        }
    }
}