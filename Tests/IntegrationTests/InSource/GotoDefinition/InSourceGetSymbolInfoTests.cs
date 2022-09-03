using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceGetSymbolInfoTests : InSourceGetSymbolInfoBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGetSymbolInfoCaller.cs");
    
    [Test]
    public void Type()
    {
        RequestAndAssertCorrectLine2(
            filePath: FilePath,
            column:13,
            line:7,
            expected:"LibraryThatReferencesLibrary.InSourceGetSymbolInfoTarget");
    }
    
    [Test]
    public void Constructor()
    {
        RequestAndAssertCorrectLine2(
            filePath: FilePath,
            column:51,
            line:7,
            expected:"LibraryThatReferencesLibrary.InSourceGetSymbolInfoTarget.InSourceGetSymbolInfoTarget()");
    }
    
    [Test]
    public void Method()
    {
        RequestAndAssertCorrectLine2(
            filePath: FilePath,
            column:17,
            line:8,
            expected:"LibraryThatReferencesLibrary.InSourceGetSymbolInfoTarget.Run()");
    }
    
    [Test]
    public void Property()
    {
        RequestAndAssertCorrectLine2(
            filePath: FilePath,
            column:17,
            line:9,
            expected:"LibraryThatReferencesLibrary.InSourceGetSymbolInfoTarget.BasicProperty");
    }
}