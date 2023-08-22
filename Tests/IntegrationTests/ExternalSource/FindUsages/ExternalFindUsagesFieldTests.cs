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
            "(?<token>_field);$",
            column: 13,
            line: 9,
            
            expected: new []
            {
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "private string _field;",
                    "ExternalFindUsagesFieldTarget",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesFieldTarget"),
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "_field = \"0\";",
                    "ExternalFindUsagesFieldTarget",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesFieldTarget"),
                new ExpectedImplementation(
                    LocationType.Decompiled,
                    "string field = _field;",
                    "ExternalFindUsagesFieldTarget",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesFieldTarget"),
            });
    }
}