using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceGotoDefinitionPropertyTests : InSourceBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGotoDefinitionPropertyCaller.cs");
    
    [Test]
    public void Setter()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column: 52,
            line: 5,
            expected: "public string BasicProperty { get; set; }",
            //TODO why does this not have a namespace
            "InSourceGotoDefinitionPropertyTarget");
    }
    
    [Test]
    public void Getter()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column: 60,
            line: 6,
            expected: "public string BasicProperty { get; set; }",
            //TODO why does this not have a namespace
            "InSourceGotoDefinitionPropertyTarget");
    }
}