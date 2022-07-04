using NUnit.Framework;
using TryOmnisharpExtension;

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
                (ResponseLocationType.Decompiled, "int num = 1;"),
                (ResponseLocationType.Decompiled, "int num2 = num + 1;"),
                (ResponseLocationType.Decompiled, "int num3 = num + num;"),
                (ResponseLocationType.Decompiled, "int num3 = num + num;"),
            });
    }
}