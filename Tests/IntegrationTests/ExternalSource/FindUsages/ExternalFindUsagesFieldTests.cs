using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesFieldTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesFieldCaller.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            "private string _field;",
            "_field",
            column: 13,
            line: 9,
            
            expected: new []
            {
                (ResponseLocationType.Decompiled, "private string _field;", "ExternalFindUsagesFieldTarget"),
                (ResponseLocationType.Decompiled, "_field = \"0\";", "ExternalFindUsagesFieldTarget"),
                (ResponseLocationType.Decompiled, "string field = _field;", "ExternalFindUsagesFieldTarget"),
            });
    }
}