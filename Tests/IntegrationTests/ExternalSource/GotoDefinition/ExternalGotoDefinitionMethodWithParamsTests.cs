using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionMethodWithParamsTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionMethodWithParamsUser.cs");

    [Test]
    public void OneParameter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 64,
            line: 9,
            expected: "public void Run(params string[] p1)",
            "LibraryThatJustReferencesFramework.ExternalGotoDefinitionMethodWithParamsTarget");
    }

    [Test]
    public void TwoParameters()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 64,
            line: 10,
            expected: "public void Run(params string[] p1)",
            "LibraryThatJustReferencesFramework.ExternalGotoDefinitionMethodWithParamsTarget");
    }
    [Test]
    public void ThreeParameters()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 64,
            line: 11,
            expected: "public void Run(params string[] p1)",
            "LibraryThatJustReferencesFramework.ExternalGotoDefinitionMethodWithParamsTarget");
    }
}