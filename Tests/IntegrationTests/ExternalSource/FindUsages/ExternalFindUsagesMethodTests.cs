using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindUsagesMethodTests : ExternalFindUsagesTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindUsagesTypeAsMethodCaller.cs");
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
                    "ExternalFindUsagesTypeAsMethodTarget a = null;",
                    "ExternalFindUsagesTypeAsMethodCaller",
                    "LibraryThatReferencesLibrary.ExternalFindUsagesTypeAsMethodCaller"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public ExternalFindUsagesTypeAsMethodTarget Run()",
                    "ExternalFindUsagesTypeAsMethodUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesTypeAsMethodUser"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public void Run(ExternalFindUsagesTypeAsMethodTarget param1)",
                    "ExternalFindUsagesTypeAsMethodUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesTypeAsMethodUser"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public ExternalFindUsagesTypeAsMethodTarget Run2(ExternalFindUsagesTypeAsMethodTarget param1)",
                    "ExternalFindUsagesTypeAsMethodUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesTypeAsMethodUser"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "public ExternalFindUsagesTypeAsMethodTarget Run2(ExternalFindUsagesTypeAsMethodTarget param1)",
                    "ExternalFindUsagesTypeAsMethodUser",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesTypeAsMethodUser"),
            });
    }
}