using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesEventTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesEventCaller.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 49,
            line: 10,
            
            expected: new []
            {
                new ExpectedImplementation(LocationType.SourceCode,
                    "new ExternalFindUsagesEventTarget().ExternalBasicEvent += OnExternalBasicEvent;",
                    "ExternalFindUsagesEventCaller",
                    "LibraryThatReferencesLibrary.ExternalFindUsagesEventCaller"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "externalFindUsagesEventTarget.ExternalBasicEvent += ObjOnExternalBasicEvent;",
                    "ExternalFindUsagesEventCaller",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesEventCaller"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "externalFindUsagesEventTarget.ExternalBasicEvent -= ObjOnExternalBasicEvent;",
                    "ExternalFindUsagesEventCaller",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesEventCaller")
            });
    }
}