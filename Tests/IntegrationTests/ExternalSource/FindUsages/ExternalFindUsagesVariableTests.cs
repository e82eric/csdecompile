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
            "(?<token>num) =",
            column: 13,
            line: 9,
            
            expected: new []
            {
                new ExpectedImplementation(LocationType.Decompiled,
                    "int num = 1;",
                    "ExternalFindUsagesVariableTarget",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesVariableTarget"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "int num2 = num + 1;",
                    "ExternalFindUsagesVariableTarget",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesVariableTarget"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "int num3 = num + num;",
                    "ExternalFindUsagesVariableTarget",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesVariableTarget"),
                new ExpectedImplementation(LocationType.Decompiled,
                    "int num3 = num + num;",
                    "ExternalFindUsagesVariableTarget",
                    "LibraryThatJustReferencesFramework.ExternalFindUsagesVariableTarget"),
            });
    }
}