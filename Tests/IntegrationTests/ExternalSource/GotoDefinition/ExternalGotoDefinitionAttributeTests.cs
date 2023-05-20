using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionAttributeTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionAttributeUser.cs");
    [Test]
    public void GotoExternalAttributeOnType()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 6,
            line: 5,
            "public class ExternalGotoDefinitionAttribute : Attribute");
    }
    [Test]
    public void GotoExternalAttributeOnMethod()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 10,
            line: 8,
            "public class ExternalGotoDefinitionAttribute : Attribute");
    }
    
    [Test]
    public void GotoExternalAttributeOnProperty()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 10,
            line: 13,
            "public class ExternalGotoDefinitionAttribute : Attribute");
    }
}