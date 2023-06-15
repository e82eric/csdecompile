using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesTypeAsAttributeTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesAttributeUser.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 6,
            line: 5,
            expected: new []
            {
                (LocationType.SourceCode,
                    "[ExternalFindUsagesAttributeTarget]",
                    "ExternalFindUsagesAttributeUser"),
                (LocationType.Decompiled,
                    "[ExternalFindUsagesAttributeTarget]",
                    "ExternalFindUsagesAttributeInternalUser"),
            });
    }
}