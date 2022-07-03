using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using NUnit.Framework;
using TryOmnisharpExtension;

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
                "public class ExternalGotoDefinitionConstructorTarget",
                "public class InSourceFindImplementationsBaseClassInheritor : InSourceFindImplementationsBaseClass"
            });
    }
    
    protected void RequestAndAssertCorrectLine(
        string filePath,
        int column,
        int line,
        IEnumerable<string> expected)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileFindImplementations,
            Arguments = new DecompiledLocationRequest
            {
                FileName = filePath,
                Column = column,
                IsDecompiled = false,
                Line = line
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, FindImplementationsResponse>(request);

        AssertInSource(response, expected);
    }
    private static void AssertInSource(
        ResponsePacket2<FindImplementationsResponse> response,
        IEnumerable<string> expected)
    {
        Assert.True(response.Success);

        foreach (var implementation in response.Body.Implementations)
        {
            Assert.AreEqual(implementation.Type, ResponseLocationType.SourceCode);

            var location = (SourceFileInfo)implementation;

            var lines = File.ReadAllLines(location.FileName);
            var line = lines[location.Line - 1].Trim();

            var fromExpected = expected.FirstOrDefault(e => e.Contains(line.Trim()));
            Assert.NotNull(fromExpected);
        }
    }
}