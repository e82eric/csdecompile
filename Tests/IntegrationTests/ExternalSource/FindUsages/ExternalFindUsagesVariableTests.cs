using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesVariableTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesVariableCaller.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            "int num = 1;",
            "num",
            column: 13,
            line: 9,
            
            expected: new []
            {
                (LocationType.Decompiled,
                    "int num = 1;",
                    "ExternalFindUsagesVariableTarget"),
                (LocationType.Decompiled,
                    "int num2 = num + 1;",
                    "ExternalFindUsagesVariableTarget"),
                (LocationType.Decompiled,
                    "int num3 = num + num;",
                    "ExternalFindUsagesVariableTarget"),
                (LocationType.Decompiled,
                    "int num3 = num + num;",
                    "ExternalFindUsagesVariableTarget"),
            });
    }
}