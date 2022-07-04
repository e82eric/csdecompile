using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionConstructorTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalGotoDefinitionConstructorCaller.cs");
    
    [Test]
    public void NoParameters()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 17,
            line: 9,
            expected: "public ExternalGotoDefinitionConstructorTarget()");
    }
    
    [Test]
    public void OneParameter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 17,
            line: 10,
            expected: "public ExternalGotoDefinitionConstructorTarget(string param1)");
    }
    
    [Test]
    public void TwoParameters()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 17,
            line: 11,
            expected: "public ExternalGotoDefinitionConstructorTarget(string param1, string param2)");
    }
}