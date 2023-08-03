using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionOfMethodParameterTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionOfMethodParameterUser.cs");

    [Test]
    public void GotoDefinition()
    {
        //Note ilSpy does not know variable names so it uses num and num2
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            filePath:FilePath,
            column:13,
            line:9,
            lineToFind:"int num = param1 \\+ 1",
            tokenToRequest:" (?<token>param1) +",
            expected:"public void Run(int param1, int param2)",
            containingTypeFullName:"LibraryThatJustReferencesFramework.ExternalGotoDefinitionOfMethodParameterTarget");
    }
}