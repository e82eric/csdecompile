using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionConstructorAttributeTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionConstructorAttributeUser.cs");
    [Test]
    public void GotoExternalAttributeOnType()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 6,
            line: 5,
            "public ExternalGotoDefinitionConstructorAttribute(string param1)");
    }
}