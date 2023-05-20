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
            tokenToRequest:"_basicField",
            expected:"private string _basicField;");
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
            tokenToRequest:"_basicField",
            expected:"private string _basicField;");
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
            lineToFind: "[ExternalGotoDefinition]",
            tokenToRequest: "ExternalGotoDefinition",
            expected: "public class ExternalGotoDefinitionAttribute : Attribute");
    }
}