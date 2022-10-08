using NUnit.Framework;
using CsDecompileLib;

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
                (LocationType.SourceCode,
                    "ExternalFindUsagesTypeAsFieldTarget a = null;",
                    "ExternalFindUsagesTypeAsFieldCaller"),
                (LocationType.Decompiled,
                    "_field = new ExternalFindUsagesTypeAsFieldTarget();",
                    "ExternalFindUsagesTypeAsFieldUser"),
                (LocationType.Decompiled,
                    "ExternalFindUsagesTypeAsFieldTarget field = _field;",
                    "ExternalFindUsagesTypeAsFieldUser"),
                (LocationType.Decompiled,
                    "private ExternalFindUsagesTypeAsFieldTarget _field;",
                    "ExternalFindUsagesTypeAsFieldUser"),
            });
    }
}