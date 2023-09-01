using NUnit.Framework;

namespace IntegrationTests;

public class ExternalGotoDefinitionOfGenericPropertyTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionOfGenericPropertyUser.cs");

    [Test]
    public void InParameter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 23,
            line: 10,
            expected: "public IReadOnlyList<TValue> Values { get; set; }",
            "LibraryThatJustReferencesFramework.ExternalGotoDefinitionOfGenericPropertyTarget`1");
    }
}