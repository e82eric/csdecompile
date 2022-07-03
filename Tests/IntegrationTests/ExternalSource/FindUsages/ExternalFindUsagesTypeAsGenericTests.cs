using NUnit.Framework;
using TryOmnisharpExtension;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesTypeAsGenericTests : ExternalFindImplementationsBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesTypeAsGenericCaller.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertLine(
            Endpoints.DecompileFindUsages,
            filePath: FilePath,
            column: 13,
            line: 9,
            expected: new []
            {
                (ResponseLocationType.SourceCode, "ExternalFindUsagesTypeAsGenericTarget a = null;"),
                (ResponseLocationType.Decompiled, "public class ExternalFindUsagesTypeAsGenericUser : ExternalFindUsagesTypeAsGenericUserBase<ExternalFindUsagesTypeAsGenericTarget>"),
            });
    }
}