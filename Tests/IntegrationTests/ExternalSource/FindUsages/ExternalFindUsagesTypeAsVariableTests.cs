using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesTypeAsVariableTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesTypeAsVariableCaller.cs");
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
                    "ExternalFindUsagesTypeAsVariableTarget a = null;",
                    "ExternalFindUsagesTypeAsVariableCaller"),
                (LocationType.Decompiled,
                    "ExternalFindUsagesTypeAsVariableTarget externalFindUsagesTypeAsVariableTarget = null;",
                    "ExternalFindUsagesTypeAsVariableCaller"),
                (LocationType.Decompiled,
                    "ExternalFindUsagesTypeAsVariableTarget externalFindUsagesTypeAsVariableTarget2 = null;",
                    "ExternalFindUsagesTypeAsVariableCaller"),
                (LocationType.Decompiled,
                    "ExternalFindUsagesTypeAsVariableTarget externalFindUsagesTypeAsVariableTarget3 = null;",
                    "ExternalFindUsagesTypeAsVariableCaller"),
            });
    }
}