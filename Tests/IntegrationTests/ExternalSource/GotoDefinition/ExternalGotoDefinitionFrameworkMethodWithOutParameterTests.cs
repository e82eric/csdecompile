using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionFrameworkMethodWithOutParameterTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionFrameworkMethodWithOutParameterUser.cs");

    [Test]
    public void OutParameter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 46,
            line: 9,
            expected: "public bool TryGetValue(TKey key, out TValue value)");
    }
}