using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesTypeAsAttributeTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesAttributeUser.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 6,
            line: 5,
            expected: new []
            {
                new ExpectedImplementation(LocationType.SourceCode,
                    "[ExternalFindUsagesAttributeTarget]",
                    "ExternalFindUsagesAttributeUser",
                    "LibraryThatReferencesLibrary.ExternalFindUsagesAttributeUser"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "[ExternalFindUsagesAttributeTarget]",
                    "ExternalFindUsagesAttributeInternalUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesAttributeInternalUser"),
            });
    }
}