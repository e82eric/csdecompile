using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionOfLocalFunctionParameterTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionOfLocalFunctionParameterUser.cs");

    [Test]
    public void GotoDefinition()
    {
        //Note ilSpy does not know variable names so it uses num and num2
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            filePath:FilePath,
            column:13,
            line:9,
            lineToFind:"return target \\+ 1;",
            tokenToRequest:"return (?<token>.*) +",
            expected:"static int addOne(int target)",
            containingTypeFullName:"LibraryThatJustReferencesFramework.ExternalGotoDefinitionOfLocalFunctionParameterTarget");
    }
}