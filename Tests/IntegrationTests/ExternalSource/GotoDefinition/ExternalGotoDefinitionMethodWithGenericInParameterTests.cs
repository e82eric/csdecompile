using NUnit.Framework;

namespace IntegrationTests;

public class ExternalGotoDefinitionMethodWithGenericInParameterTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionMethodWithGenericInParameterUser.cs");

    [Test]
    public void InParameter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 92,
            line: 11,
            expected: "public bool TryRun(T val, in T2 result)",
            "LibraryThatJustReferencesFramework.ExternalGotoDefinitionMethodWithGenericInParameterTarget`2");
    }
}