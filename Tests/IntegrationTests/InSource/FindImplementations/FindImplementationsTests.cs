using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class FindImplementationsTests : FindImplementationsTestsBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "InSourceFindImplementationsBaseClassInheritor.cs");
    [Test]
    public void Test1()
    {
        RequestAndAssertCorrectLine(
            command: Endpoints.DecompileFindImplementations,
            filePath:FilePath,
            column:62,
            line:1,
            new []
            {
                //I guess right now we are including the declaration in the response
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public class InSourceFindImplementationsBaseClass",
                    "InSourceFindImplementationsBaseClass",
                    "LibraryThatReferencesLibrary.InSourceFindImplementationsBaseClass"),
                new ExpectedImplementation(
                    LocationType.SourceCode,
                    "public class InSourceFindImplementationsBaseClassInheritor : InSourceFindImplementationsBaseClass",
                    "InSourceFindImplementationsBaseClassInheritor",
                    "LibraryThatReferencesLibrary.InSourceFindImplementationsBaseClassInheritor")
            });
    }
}