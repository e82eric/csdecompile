using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionMethodWithOutParameterTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionMethodWithOutParameterUser.cs");

    [Test]
    public void OutParameter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 70,
            line: 9,
            expected: "public bool TryRun(string val, out string result)",
            "LibraryThatJustReferencesFramework.ExternalGotoDefinitionMethodWithOutParameterTarget");
    }
}