using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceFindImplementationsMethodWithGenericOutParametersTests : FindImplementationsTestsBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "InSourceFindImplementationsMethodWithGenericOutParametersTarget.cs");
    [Test]
    public void Test1()
    {
        RequestAndAssertCorrectLine(
            command: Endpoints.DecompileFindImplementations,
            filePath:FilePath,
            column:14,
            line:5,
            new []
            {
                //I guess right now we are including the declaration in the response
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public bool TryRun(T1 t1, out T2 t2)",
                    "InSourceFindImplementationsMethodWithGenericOutParametersImplementationWithGenerics",
                    "LibraryThatReferencesLibrary.InSourceFindImplementationsMethodWithGenericOutParametersImplementationWithGenerics`2"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public bool TryRun(string t1, out object t2)",
                    "InSourceFindImplementationsMethodWithGenericOutParametersImplementationWithTypes",
                    "LibraryThatReferencesLibrary.InSourceFindImplementationsMethodWithGenericOutParametersImplementationWithTypes"),
            });
    }
}