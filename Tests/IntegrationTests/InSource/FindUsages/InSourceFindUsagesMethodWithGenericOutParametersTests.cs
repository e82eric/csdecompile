using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceFindUsagesMethodWithGenericOutParametersTests : InSourceFindUsagesBase
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
                    "LibraryThatReferencesLibrary.InSourceFindUsagesMethodWithGenericOutParametersUser"),
                ("b.TryRun(default, out _);",
                    "InSourceFindUsagesMethodWithGenericOutParametersUser",
                    "LibraryThatReferencesLibrary.InSourceFindUsagesMethodWithGenericOutParametersUser"),
                ("public bool TryRun(T1 t1, out T2 t2)",
                    "InSourceFindUsagesMethodWithGenericOutParametersTarget",
                    "LibraryThatReferencesLibrary.InSourceFindUsagesMethodWithGenericOutParametersTarget`2"),
            });
    }
}