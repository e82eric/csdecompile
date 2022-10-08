using NUnit.Framework;
using CsDecompileLib;

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
                (LocationType.SourceCode,
                    "ExternalFindUsagesPropertyTarget a = null;",
                    "ExternalFindUsagesPropertyCaller"),
                (LocationType.Decompiled,
                    "public ExternalFindUsagesPropertyTarget BasicProperty { get; set; }",
                    "ExternalFindUsagesPropertyUser"),
            });
    }
}