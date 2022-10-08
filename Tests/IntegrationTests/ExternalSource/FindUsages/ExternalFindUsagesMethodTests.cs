using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesMethodTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesTypeAsMethodCaller.cs");
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
                    "ExternalFindUsagesTypeAsMethodTarget a = null;",
                    "ExternalFindUsagesTypeAsMethodCaller"),
                (LocationType.Decompiled,
                    "public ExternalFindUsagesTypeAsMethodTarget Run()",
                    "ExternalFindUsagesTypeAsMethodUser"),
                (LocationType.Decompiled,
                    "public void Run(ExternalFindUsagesTypeAsMethodTarget param1)",
                    "ExternalFindUsagesTypeAsMethodUser"),
                (LocationType.Decompiled,
                    "public ExternalFindUsagesTypeAsMethodTarget Run2(ExternalFindUsagesTypeAsMethodTarget param1)",
                    "ExternalFindUsagesTypeAsMethodUser"),
                (LocationType.Decompiled,
                    "public ExternalFindUsagesTypeAsMethodTarget Run2(ExternalFindUsagesTypeAsMethodTarget param1)",
                    "ExternalFindUsagesTypeAsMethodUser"),
            });
    }
}