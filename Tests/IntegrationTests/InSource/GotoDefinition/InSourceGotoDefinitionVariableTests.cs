using CsDecompileLib;
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
            expected: new ExpectedImplementation(
                LocationType.SourceCode,
                "var a = 1;",
                null,
                //TODO why does this not have a namespace
                "InSourceGotoDefinitionVariableTarget"));
    }
}