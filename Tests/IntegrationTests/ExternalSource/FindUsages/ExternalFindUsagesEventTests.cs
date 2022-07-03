using NUnit.Framework;
using TryOmnisharpExtension;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesEventTests : ExternalFindImplementationsBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesEventCaller.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertLine(
            Endpoints.DecompileFindUsages,
            filePath: FilePath,
            column: 49,
            line: 10,
            
            expected: new []
            {
                (ResponseLocationType.SourceCode,
                    "new ExternalFindUsagesEventTarget().ExternalBasicEvent += OnExternalBasicEvent;"),
                (ResponseLocationType.Decompiled,
                    "obj.ExternalBasicEvent += ObjOnExternalBasicEvent;"),
                (ResponseLocationType.Decompiled,
                    "obj.ExternalBasicEvent -= ObjOnExternalBasicEvent")
            });
    }
}