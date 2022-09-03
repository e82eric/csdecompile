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
                (ResponseLocationType.SourceCode,
                    "ExternalFindUsagesTypeAsFieldTarget a = null;",
                    "ExternalFindUsagesTypeAsFieldCaller"),
                (ResponseLocationType.Decompiled,
                    "_field = new ExternalFindUsagesTypeAsFieldTarget();",
                    "ExternalFindUsagesTypeAsFieldUser"),
                (ResponseLocationType.Decompiled,
                    "ExternalFindUsagesTypeAsFieldTarget field = _field;",
                    "ExternalFindUsagesTypeAsFieldUser"),
                (ResponseLocationType.Decompiled,
                    "private ExternalFindUsagesTypeAsFieldTarget _field;",
                    "ExternalFindUsagesTypeAsFieldUser"),
            });
    }
}