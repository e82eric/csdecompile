using NUnit.Framework;
using TryOmnisharpExtension;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesFieldTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesFieldCaller.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            "private string _field;",
            "_field",
            column: 13,
            line: 9,
            
            expected: new []
            {
                (ResponseLocationType.Decompiled, "private string _field;"),
                (ResponseLocationType.Decompiled, "_field = 0;"),
                (ResponseLocationType.Decompiled, "string field = _field;"),
            });
    }
}