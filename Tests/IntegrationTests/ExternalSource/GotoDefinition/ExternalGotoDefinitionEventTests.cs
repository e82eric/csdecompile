using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionEventTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionEventCaller.cs");
    
    [Test]
    public void Setter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 17,
            line: 11,
            expected: "public event EventHandler BasicEvent;");
    }
    
    [Test]
    public void Getter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 17,
            line: 12,
            expected: "public event EventHandler BasicEvent;");
    }

    [Test]
    public void Publish()
    {
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            filePath:FilePath,
            column:13,
            line:10,
            lineToFind:"BasicEvent\\(this, EventArgs.Empty\\);",
            tokenToRequest:"(?<token>BasicEvent)\\(",
            expected:"public event EventHandler BasicEvent;");
    }
}