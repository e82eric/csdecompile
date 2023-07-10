using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionMethodWithGenericOutParameterTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionMethodWithGenericOutParameterUser.cs");

    [Test]
    public void OutParameter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 93,
            line: 9,
            expected: "public bool TryRun(T val, out T2 result)");
    }
}