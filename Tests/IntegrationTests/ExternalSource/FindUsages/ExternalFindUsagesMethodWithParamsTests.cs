using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesMethodWithParamsTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesMethodWithParamsUser.cs");
    [Test]
    public void FindUsages()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 60,
            line: 9,
            expected: new []
            {
                (LocationType.SourceCode,
                    "new ExternalFindUsagesMethodWithParamsTarget().Run(\"Test\");",
                    "ExternalFindUsagesMethodWithParamsUser"),
                (LocationType.SourceCode,
                    "new ExternalFindUsagesMethodWithParamsTarget().Run(\"Test\", \"More\");",
                    "ExternalFindUsagesMethodWithParamsUser"),
                (LocationType.SourceCode,
                    "new ExternalFindUsagesMethodWithParamsTarget().Run(\"Test\", \"More\", \"Params\");",
                    "ExternalFindUsagesMethodWithParamsUser"),
            });
    }
}