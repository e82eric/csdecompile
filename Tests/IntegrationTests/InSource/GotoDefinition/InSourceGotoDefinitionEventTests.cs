using CsDecompileLib;
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
            expected: new ExpectedImplementation(
                LocationType.SourceCode,
                "public event EventHandler<EventArgs> BasicEvent;",
                //TODO why does this not have a namespace
                "InSourceGotoDefinitionEventTarget",
                "InSourceGotoDefinitionEventTarget"));
    }
    
    [Test]
    public void Subscribe()
    {
        RequestAndAssertCorrectLine(
            filePath: CallerFilePath,
            column: 13,
            line: 8,
            expected: new ExpectedImplementation(
                LocationType.SourceCode,
                "public event EventHandler<EventArgs> BasicEvent;",
                "InSourceGotoDefinitionEventTarget",
                //TODO why does this not have a namespace
                "InSourceGotoDefinitionEventTarget"));
    }
    
    [Test]
    public void Unsubscribe()
    {
        RequestAndAssertCorrectLine(
            filePath: CallerFilePath,
            column: 13,
            line: 9,
            expected: new ExpectedImplementation(
                LocationType.SourceCode,
                "public event EventHandler<EventArgs> BasicEvent;",
                "InSourceGotoDefinitionEventTarget",
                //TODO why does this not have a namespace
                "InSourceGotoDefinitionEventTarget"));
    }
}