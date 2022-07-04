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
                (ResponseLocationType.SourceCode, "ExternalFindUsagesTypeAsMethodTarget a = null;"),
                (ResponseLocationType.Decompiled, "public ExternalFindUsagesTypeAsMethodTarget Run()"),
                (ResponseLocationType.Decompiled, "public void Run(ExternalFindUsagesTypeAsMethodTarget param1)"),
                (ResponseLocationType.Decompiled, "public ExternalFindUsagesTypeAsMethodTarget Run2(ExternalFindUsagesTypeAsMethodTarget param1)"),
                (ResponseLocationType.Decompiled, "public ExternalFindUsagesTypeAsMethodTarget Run2(ExternalFindUsagesTypeAsMethodTarget param1)"),
            });
    }
}