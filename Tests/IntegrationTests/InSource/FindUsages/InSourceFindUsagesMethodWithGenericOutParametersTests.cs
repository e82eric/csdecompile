using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceFindUsagesMethodWithGenericOutParametersTests : FindImplementationsTestsBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "InSourceFindUsagesMethodWithGenericOutParametersTarget.cs");
    [Test]
    public void Test1()
    {
        RequestAndAssertCorrectLine(
            command: Endpoints.DecompileFindUsages,
            filePath:FilePath,
            column:21,
            line:5,
            new []
            {
                //I guess right now we are including the declaration in the response
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "a.TryRun(default, out _);",
                    "InSourceFindUsagesMethodWithGenericOutParametersUser",
                    "LibraryThatReferencesLibrary.InSourceFindUsagesMethodWithGenericOutParametersUser"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "b.TryRun(default, out _);",
                    "InSourceFindUsagesMethodWithGenericOutParametersUser",
                    "LibraryThatReferencesLibrary.InSourceFindUsagesMethodWithGenericOutParametersUser"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public bool TryRun(T1 t1, out T2 t2)",
                    "InSourceFindUsagesMethodWithGenericOutParametersTarget",
                    "LibraryThatReferencesLibrary.InSourceFindUsagesMethodWithGenericOutParametersTarget`2"),
            });
    }
}