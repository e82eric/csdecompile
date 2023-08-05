using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionOfLambdaParameterTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionOfLambdaParameterUser.cs");

    [Test]
    public void GotoDefinition()
    {
        //Note ilSpy does not know variable names so it uses num and num2
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            filePath:FilePath,
            column:13,
            line:9,
            lineToFind:"int num = i \\+ 1;",
            tokenToRequest:"num = (?<token>i) +",
            expected:"Run(delegate(int i)",
            containingTypeFullName:"LibraryThatJustReferencesFramework.ExternalGotoDefinitionOfLambdaParameterTarget");
    }
}