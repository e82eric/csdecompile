using NUnit.Framework;
using TryOmnisharpExtension;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesPropertyTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesPropertyCaller.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 13,
            line: 9,
            expected: new []
            {
                (ResponseLocationType.SourceCode,
                    "ExternalFindUsagesPropertyTarget a = null;",
                    "ExternalFindUsagesPropertyCaller"),
                (ResponseLocationType.Decompiled,
                    "public ExternalFindUsagesPropertyTarget BasicProperty { get; set; }",
                    "ExternalFindUsagesPropertyUser"),
            });
    }
}