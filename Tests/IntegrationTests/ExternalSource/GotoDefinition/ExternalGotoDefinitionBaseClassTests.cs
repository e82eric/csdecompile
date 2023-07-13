using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionBaseClassTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionBaseClassCaller.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 58,
            line: 5,
            "public class ExternalGotoDefinitionBaseClassTarget",
            "LibraryThatJustReferencesFramework.ExternalGotoDefinitionBaseClassTarget");
    }
}