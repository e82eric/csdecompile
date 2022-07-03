using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionMethodTests : ExternalTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionMethodCaller.cs");
    
    [Test]
    public void NoParameters()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 54,
            line: 10,
            expected: "public void ExternalBasicMethod()");
    }
    
    [Test]
    public void OneParameter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 54,
            line: 11,
            expected: "public void ExternalBasicMethod(string param1)");
    }
    
    [Test]
    public void TwoParameters()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 54,
            line: 12,
            expected: "public void ExternalBasicMethod(string param1, string param2)");
    }
}