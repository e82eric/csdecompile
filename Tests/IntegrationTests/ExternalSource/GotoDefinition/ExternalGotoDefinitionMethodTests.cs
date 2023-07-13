using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionMethodTests : ExternalGotoDefinitionTestBase
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
            expected: "public void ExternalBasicMethod()",
            "LibraryThatJustReferencesFramework.ExternalGotoDefinitionMethodTarget");
    }
    
    [Test]
    public void OneParameter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 54,
            line: 11,
            expected: "public void ExternalBasicMethod(string param1)",
            "LibraryThatJustReferencesFramework.ExternalGotoDefinitionMethodTarget");
    }
    
    [Test]
    public void TwoParameters()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 54,
            line: 12,
            expected: "public void ExternalBasicMethod(string param1, string param2)",
            "LibraryThatJustReferencesFramework.ExternalGotoDefinitionMethodTarget");
    }
}