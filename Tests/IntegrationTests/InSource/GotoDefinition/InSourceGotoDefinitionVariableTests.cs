using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceGotoDefinitionVariableTests : InSourceBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGotoDefinitionVariableTarget.cs");
    
    [Test]
    public void SimpleUsage()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column: 17,
            line: 6,
            expected: "var a = 1;",
            //TODO why does this not have a namespace
            "InSourceGotoDefinitionVariableTarget");
    }
}