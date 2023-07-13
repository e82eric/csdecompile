using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionFieldTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionFieldCaller.cs");
    
    [Test]
    public void SetValue()
    {
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            filePath:FilePath,
            column:13,
            line:9,
            lineToFind:"_basicField = \"0\"",
            tokenToRequest:"(?<token>_basicField) =",
            expected:"private string _basicField;",
            containingTypeFullName:"LibraryThatJustReferencesFramework.ExternalGotoDefinitionFieldTarget");
    }
    [Test]
    public void GetValue()
    {
        //Note ilSpy does not know variable names so in this case it uses the fieldName to create
        //the variable name
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            filePath:FilePath,
            column:13,
            line:9,
            lineToFind:"string basicField = _basicField",
            tokenToRequest:" (?<token>_basicField)",
            expected:"private string _basicField;",
            containingTypeFullName:"LibraryThatJustReferencesFramework.ExternalGotoDefinitionFieldTarget");
    }
}

[TestFixture]
public class ExternalGotoDefinitionAttributeInternalUserUserTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionAttributeInternalUserUser.cs");

    [Test]
    public void Test()
    {
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            filePath: FilePath,
            column: 17,
            line: 9,
            lineToFind: "\\[ExternalGotoDefinition\\]",
            tokenToRequest: "\\[(?<token>ExternalGotoDefinition)\\]",
            expected: "public class ExternalGotoDefinitionAttribute : Attribute",
            containingTypeFullName:"LibraryThatJustReferencesFramework.ExternalGotoDefinitionAttribute");
    }
}