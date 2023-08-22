using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesTypeAsFieldTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesTypeAsFieldCaller.cs");
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
                    "ExternalFindUsagesTypeAsFieldTarget a = null;",
                    "ExternalFindUsagesTypeAsFieldCaller",
                    "LibraryThatReferencesLibrary.ExternalFindUsagesTypeAsFieldCaller"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "_field = new ExternalFindUsagesTypeAsFieldTarget();",
                    "ExternalFindUsagesTypeAsFieldUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesTypeAsFieldUser"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "ExternalFindUsagesTypeAsFieldTarget field = _field;",
                    "ExternalFindUsagesTypeAsFieldUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesTypeAsFieldUser"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "private ExternalFindUsagesTypeAsFieldTarget _field;",
                    "ExternalFindUsagesTypeAsFieldUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesTypeAsFieldUser"),
            });
    }
}