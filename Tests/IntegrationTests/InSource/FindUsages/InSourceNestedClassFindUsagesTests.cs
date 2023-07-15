using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceNestedClassFindUsagesTests : InSourceFindUsagesBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "InSourceNestedClassFindUsagesUser.cs");

    [Test]
    public void Test1()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column: 49,
            line: 7,
            new[]
            {
                //I guess right now we are including the declaration in the response
                ("public class InnerClass",
                    "InSourceNestedClassFindUsagesTarget",
                    "LibraryThatReferencesLibrary.InSourceNestedClassFindUsagesTarget"),
                ("InSourceNestedClassFindUsagesTarget.InnerClass a;",
                    "InSourceNestedClassFindUsagesUser",
                    "LibraryThatReferencesLibrary.InSourceNestedClassFindUsagesUser"),
                ("InnerClass b;",
                    "InSourceNestedClassFindUsagesTarget",
                    "LibraryThatReferencesLibrary.InSourceNestedClassFindUsagesTarget"),
            });
    }
}