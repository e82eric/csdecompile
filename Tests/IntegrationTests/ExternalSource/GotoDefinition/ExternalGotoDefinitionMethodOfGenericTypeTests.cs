using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionMethodOfGenericTypeTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionMethodOfGenericTypeCaller.cs");
    
    [Test]
    public void NoParameters()
    {
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            filePath: FilePath,
            column: 17,
            line: 9,
            "externalGotoDefinitionMethodOfGenericTypeTarget.ExternalBasicMethod(string.Empty);",
            "ExternalBasicMethod",
            expected: "public void ExternalBasicMethod(T param1)");
    }
}