using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceNestedClassFindUsagesTests : FindImplementationsTestsBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "InSourceNestedClassFindUsagesUser.cs");

    [Test]
    public void Test1()
    {
        RequestAndAssertCorrectLine(
            command: Endpoints.DecompileFindUsages,
            filePath: FilePath,
            column: 49,
            line: 7,
            new[]
            {
                //I guess right now we are including the declaration in the response
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public class InnerClass",
                    "InSourceNestedClassFindUsagesTarget",
                    "LibraryThatReferencesLibrary.InSourceNestedClassFindUsagesTarget"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "InSourceNestedClassFindUsagesTarget.InnerClass a;",
                    "InSourceNestedClassFindUsagesUser",
                    "LibraryThatReferencesLibrary.InSourceNestedClassFindUsagesUser"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "InnerClass b;",
                    "InSourceNestedClassFindUsagesTarget",
                    "LibraryThatReferencesLibrary.InSourceNestedClassFindUsagesTarget"),
            });
    }
}