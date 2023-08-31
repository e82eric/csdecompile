using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class GetInterfaceMembersTests : FindImplementationsTestsBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "InSourceGetInterfaceMembersTarget.cs");
    [Test]
    public void GetSimpleMembers()
    {
        RequestAndAssertCorrectLine(
            Endpoints.GetTypeMembers,
            filePath:FilePath,
            column:9,
            line:7,
            new []
            {
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "void Method1();",
                    "InSourceGetInterfaceMembersTarget",
                    "LibraryThatReferencesLibrary.InSourceGetInterfaceMembersTarget"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "string Prop1 { get; set; }",
                    "InSourceGetInterfaceMembersTarget",
                    "LibraryThatReferencesLibrary.InSourceGetInterfaceMembersTarget"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "IEnumerable<string> Prop2 { get; set; }",
                    "InSourceGetInterfaceMembersTarget",
                    "LibraryThatReferencesLibrary.InSourceGetInterfaceMembersTarget"),
            });
    }

    [Test]
    public void GetFromOutsideClass()
    {
        RequestAndAssertCorrectLine(
            Endpoints.GetTypeMembers,
            filePath:FilePath,
            column:1,
            line:1,
            new []
            {
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "void Method1();",
                    "InSourceGetInterfaceMembersTarget",
                    "LibraryThatReferencesLibrary.InSourceGetInterfaceMembersTarget"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "string Prop1 { get; set; }",
                    "InSourceGetInterfaceMembersTarget",
                    "LibraryThatReferencesLibrary.InSourceGetInterfaceMembersTarget"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "IEnumerable<string> Prop2 { get; set; }",
                    "InSourceGetInterfaceMembersTarget",
                    "LibraryThatReferencesLibrary.InSourceGetInterfaceMembersTarget"),
            });
    }
}