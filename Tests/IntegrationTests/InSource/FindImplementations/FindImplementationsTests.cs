using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class FindImplementationsTests
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "InSourceFindImplementationsBaseClassInheritor.cs");
    [Test]
    public void Test1()
    {
        RequestAndAssertCorrectLine(
            filePath:FilePath,
            column:62,
            line:1,
            new []
            {
                //I guess right now we are including the declaration in the response
                ("public class InSourceFindImplementationsBaseClass",
                    "InSourceFindImplementationsBaseClass",
                    "LibraryThatReferencesLibrary.InSourceFindImplementationsBaseClass"),
                ("public class InSourceFindImplementationsBaseClassInheritor : InSourceFindImplementationsBaseClass",
                    "InSourceFindImplementationsBaseClassInheritor",
                    "LibraryThatReferencesLibrary.InSourceFindImplementationsBaseClassInheritor")
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
            Command = Endpoints.DecompileFindImplementations,
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