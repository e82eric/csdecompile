using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceGotoDefinitionFieldTests : InSourceBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGotoDefinitionFieldTarget.cs");
    
    [Test]
    public void Setter()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column: 9,
            line: 7,
            expected: new ExpectedImplementation(
                LocationType.SourceCode,
                "private string _basicField;",
                null,
                //TODO why does this not have a namespace
                "InSourceGotoDefinitionFieldTarget"));
    }
    
    [Test]
    public void Getter()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column: 17,
            line: 12,
            expected: new ExpectedImplementation(
                LocationType.SourceCode,
                "private string _basicField;",
                null,
                //TODO why does this not have a namespace
                "InSourceGotoDefinitionFieldTarget"));
    }
}
