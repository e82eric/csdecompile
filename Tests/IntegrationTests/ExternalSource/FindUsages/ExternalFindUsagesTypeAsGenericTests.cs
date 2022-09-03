using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesTypeAsGenericTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesTypeAsGenericCaller.cs");
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
                    "ExternalFindUsagesTypeAsGenericTarget a = null;",
                    "ExternalFindUsagesTypeAsGenericCaller"),
                (ResponseLocationType.Decompiled,
                    "public class ExternalFindUsagesTypeAsGenericUser : ExternalFindUsagesTypeAsGenericUserBase<ExternalFindUsagesTypeAsGenericTarget>",
                    "ExternalFindUsagesTypeAsGenericUser"),
            });
    }
}