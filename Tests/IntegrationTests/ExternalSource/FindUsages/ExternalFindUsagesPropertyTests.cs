using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesPropertyTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesPropertyCaller.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 13,
            line: 9,
            expected: new []
            {
                new ExpectedImplementation(LocationType.SourceCode,
                    "ExternalFindUsagesPropertyTarget a = null;",
                    "ExternalFindUsagesPropertyCaller",
                    "LibraryThatReferencesLibrary.ExternalFindUsagesPropertyCaller"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public ExternalFindUsagesPropertyTarget BasicProperty { get; set; }",
                    "ExternalFindUsagesPropertyUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesPropertyUser"),
            });
    }
}