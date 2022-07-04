using NUnit.Framework;
using TryOmnisharpExtension;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesTypeAsFieldTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesTypeAsFieldCaller.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 13,
            line: 9,
            expected: new []
            {
                (ResponseLocationType.SourceCode, "ExternalFindUsagesTypeAsFieldTarget a = null;"),
                (ResponseLocationType.Decompiled, "_field = new ExternalFindUsagesTypeAsFieldTarget();"),
                (ResponseLocationType.Decompiled, "ExternalFindUsagesTypeAsFieldTarget a = _field;"),
                (ResponseLocationType.Decompiled, "private ExternalFindUsagesTypeAsFieldTarget _field;"),
            });
    }
}