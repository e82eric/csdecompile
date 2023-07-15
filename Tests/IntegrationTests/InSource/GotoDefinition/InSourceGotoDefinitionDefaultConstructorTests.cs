using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceGotoDefinitionDefaultConstructorTests : InSourceBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGotoDefinitionDefaultConstructorCaller.cs");
    [Test]
    public void NoParameters()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column:17,
            line:7,
            expected:"public class InSourceGotoDefinitionDefaultConstructorTarget",
            "LibraryThatReferencesLibrary.InSourceGotoDefinitionDefaultConstructorTarget");
    }
}