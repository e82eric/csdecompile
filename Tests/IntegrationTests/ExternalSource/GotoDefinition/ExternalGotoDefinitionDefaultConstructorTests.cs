using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionDefaultConstructorTests : ExternalTestBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionDefaultConstructorCaller.cs");
    
    [Test]
    public void NoParameters()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 17,
            line: 9,
            expected: "public class ExternalGotoDefinitionDefaultConstructorTarget");
    }
}