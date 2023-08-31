using System.IO;
using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class GetTypeMembersTests : FindImplementationsTestsBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "InSourceGetMembersTarget.cs");
    [Test]
    public void GetSimpleMembers()
    {
        RequestAndAssertCorrectLine(
            Endpoints.GetTypeMembers,
            filePath:FilePath,
            column:5,
            line:3,
            new []
            {
                //I guess right now we are including the declaration in the response
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public InSourceGetMembersTarget()",
                    "InSourceGetMembersTarget",
                    "LibraryThatReferencesLibrary.InSourceGetMembersTarget"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public string BasicProperty { get; set; }",
                    "InSourceGetMembersTarget",
                    "LibraryThatReferencesLibrary.InSourceGetMembersTarget"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public void BasicMethod()",
                    "InSourceGetMembersTarget",
                    "LibraryThatReferencesLibrary.InSourceGetMembersTarget"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public void BasicMethod(string param1)",
                    "InSourceGetMembersTarget",
                    "LibraryThatReferencesLibrary.InSourceGetMembersTarget"),
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
                //I guess right now we are including the declaration in the response
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public InSourceGetMembersTarget()",
                    "InSourceGetMembersTarget",
                    "LibraryThatReferencesLibrary.InSourceGetMembersTarget"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public string BasicProperty { get; set; }",
                    "InSourceGetMembersTarget",
                    "LibraryThatReferencesLibrary.InSourceGetMembersTarget"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public void BasicMethod()",
                    "InSourceGetMembersTarget",
                    "LibraryThatReferencesLibrary.InSourceGetMembersTarget"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public void BasicMethod(string param1)",
                    "InSourceGetMembersTarget",
                    "LibraryThatReferencesLibrary.InSourceGetMembersTarget"),
            });
    }
}