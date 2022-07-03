using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionVariableTests : ExternalTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionVariableCaller.cs");
    
    [Test]
    public void SetValue()
    {
        //Note ilSpy does not know variable names so it uses num and num2
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            filePath:FilePath,
            column:13,
            line:9,
            lineToFind:"int num2 = num + 1",
            tokenToRequest:"num ",
            expected:"int num = 0;");
    }
}