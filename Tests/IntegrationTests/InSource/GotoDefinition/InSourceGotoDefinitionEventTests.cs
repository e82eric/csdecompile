using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceGotoDefinitionEventTests : InSourceBase
{
    private static string TargetFilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGotoDefinitionEventTarget.cs");
    
    private static string CallerFilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGotoDefinitionEventCaller.cs");
    
    [Test]
    public void Publish()
    {
        RequestAndAssertCorrectLine(
            filePath: TargetFilePath,
            column: 9,
            line: 9,
            expected: "public event EventHandler<EventArgs> BasicEvent;",
            null);
    }
    
    [Test]
    public void Subscribe()
    {
        RequestAndAssertCorrectLine(
            filePath: CallerFilePath,
            column: 13,
            line: 8,
            expected: "public event EventHandler<EventArgs> BasicEvent;",
            null);
    }
    
    [Test]
    public void Unsubscribe()
    {
        RequestAndAssertCorrectLine(
            filePath: CallerFilePath,
            column: 13,
            line: 9,
            expected: "public event EventHandler<EventArgs> BasicEvent;",
            null);
    }
}