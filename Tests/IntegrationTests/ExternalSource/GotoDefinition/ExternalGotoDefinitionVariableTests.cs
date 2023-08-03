using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionVariableTests : ExternalGotoDefinitionTestBase
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
            lineToFind:"int num2 = num \\+ 1",
            tokenToRequest:" (?<token>num) +",
            expected:"int num = 0;",
            containingTypeFullName:"LibraryThatJustReferencesFramework.ExternalGotoDefinitionVariableTarget");
    }
}