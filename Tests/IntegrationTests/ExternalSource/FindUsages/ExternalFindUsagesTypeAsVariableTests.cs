using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesTypeAsVariableTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesTypeAsVariableCaller.cs");
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
                    "ExternalFindUsagesTypeAsVariableTarget a = null;",
                    "ExternalFindUsagesTypeAsVariableCaller",
                    "LibraryThatReferencesLibrary.ExternalFindUsagesTypeAsVariableCaller"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "ExternalFindUsagesTypeAsVariableTarget externalFindUsagesTypeAsVariableTarget = null;",
                    "ExternalFindUsagesTypeAsVariableCaller",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesTypeAsVariableCaller"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "ExternalFindUsagesTypeAsVariableTarget externalFindUsagesTypeAsVariableTarget2 = null;",
                    "ExternalFindUsagesTypeAsVariableCaller",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesTypeAsVariableCaller"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "ExternalFindUsagesTypeAsVariableTarget externalFindUsagesTypeAsVariableTarget3 = null;",
                    "ExternalFindUsagesTypeAsVariableCaller",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesTypeAsVariableCaller"),
            });
    }
}