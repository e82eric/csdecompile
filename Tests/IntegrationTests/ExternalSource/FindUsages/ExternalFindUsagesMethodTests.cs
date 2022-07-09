using NUnit.Framework;
using TryOmnisharpExtension;

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
                (ResponseLocationType.SourceCode,
                    "ExternalFindUsagesTypeAsMethodTarget a = null;",
                    "ExternalFindUsagesTypeAsMethodCaller"),
                (ResponseLocationType.Decompiled,
                    "public ExternalFindUsagesTypeAsMethodTarget Run()",
                    "ExternalFindUsagesTypeAsMethodUser"),
                (ResponseLocationType.Decompiled,
                    "public void Run(ExternalFindUsagesTypeAsMethodTarget param1)",
                    "ExternalFindUsagesTypeAsMethodUser"),
                (ResponseLocationType.Decompiled,
                    "public ExternalFindUsagesTypeAsMethodTarget Run2(ExternalFindUsagesTypeAsMethodTarget param1)",
                    "ExternalFindUsagesTypeAsMethodUser"),
                (ResponseLocationType.Decompiled,
                    "public ExternalFindUsagesTypeAsMethodTarget Run2(ExternalFindUsagesTypeAsMethodTarget param1)",
                    "ExternalFindUsagesTypeAsMethodUser"),
            });
    }
}