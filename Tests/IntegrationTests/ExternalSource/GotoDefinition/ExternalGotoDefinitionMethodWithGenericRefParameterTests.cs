using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionMethodWithGenericRefParameterTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionMethodWithGenericRefParameterUser.cs");

    [Test]
    public void Parameter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 93,
            line: 10,
            expected: "public bool TryRun(T val, ref T2 result)",
            "LibraryThatJustReferencesFramework.ExternalGotoDefinitionMethodWithGenericRefParameterTarget`2");
    }
}