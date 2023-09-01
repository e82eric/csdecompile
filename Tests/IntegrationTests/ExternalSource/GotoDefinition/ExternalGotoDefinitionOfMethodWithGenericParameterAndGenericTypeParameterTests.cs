using NUnit.Framework;

namespace IntegrationTests;

public class ExternalGotoDefinitionOfMethodWithGenericParameterAndGenericTypeParameterTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionOfMethodWithGenericParameterAndGenericTypeParameterUser.cs");

    [Test]
    public void TypeParameterSpecified()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 15,
            line: 10,
            expected: "public void ExternalBasicMethod<T2>(T param1, T2 param2)",
            "LibraryThatJustReferencesFramework.ExternalGotoDefinitionOfMethodWithGenericParameterAndGenericTypeParameterTarget`1");
    }
    [Test]
    public void TypeParameterInfered()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 15,
            line: 11,
            expected: "public void ExternalBasicMethod<T2>(T param1, T2 param2)",
            "LibraryThatJustReferencesFramework.ExternalGotoDefinitionOfMethodWithGenericParameterAndGenericTypeParameterTarget`1");
    }
}