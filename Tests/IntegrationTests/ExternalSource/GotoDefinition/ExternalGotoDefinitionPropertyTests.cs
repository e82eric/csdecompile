using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionPropertyTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionPropertyCaller.cs");
    
    [Test]
    public void Setter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 56,
            line: 9,
            expected: "public string ExternalBasicProperty { get; set; }");
    }
    
    [Test]
    public void Getter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 64,
            line: 10,
            expected: "public string ExternalBasicProperty { get; set; }");
    }
}