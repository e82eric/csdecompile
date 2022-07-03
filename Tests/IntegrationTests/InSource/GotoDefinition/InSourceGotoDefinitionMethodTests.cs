using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceGotoDefinitionMethodTests : InSourceBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGotoDefinitionMethodCaller.cs");
    
    [Test]
    public void NoParameters()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column:54,
            line:9,
            expected:"public void BasicMethod()");
    }
    
    [Test]
    public void OneParameter()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column:54,
            line:10,
            expected:"public void BasicMethod(string param1)");
    }
    
    [Test]
    public void TwoParameters()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column:54,
            line:11,
            expected:"public void BasicMethod(string param1, string param2)");
    }
}