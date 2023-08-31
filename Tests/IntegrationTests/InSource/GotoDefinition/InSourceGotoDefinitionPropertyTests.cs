using CsDecompileLib;
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
            expected: new ExpectedImplementation(
                LocationType.SourceCode,
                "public string BasicProperty { get; set; }",
                "InSourceGotoDefinitionPropertyTarget",
                //TODO why does this not have a namespace
                "InSourceGotoDefinitionPropertyTarget"));
    }
    
    [Test]
    public void Getter()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column: 60,
            line: 6,
            expected: new ExpectedImplementation(
                LocationType.SourceCode,
                "public string BasicProperty { get; set; }",
                "InSourceGotoDefinitionPropertyTarget",
                //TODO why does this not have a namespace
                "InSourceGotoDefinitionPropertyTarget"));
    }
}