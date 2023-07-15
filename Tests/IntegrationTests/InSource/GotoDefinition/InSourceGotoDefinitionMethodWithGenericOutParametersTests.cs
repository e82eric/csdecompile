using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceGotoDefinitionMethodWithGenericOutParametersTests : InSourceBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGotoDefinitionMethodWithGenericOutParametersUser.cs");

    [Test]
    public void GenericParameters()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column:15,
            line:8,
            expected:"public bool TryRun(T1 t1, out T2 t2)",
            "LibraryThatReferencesLibrary.InSourceGotoDefinitionMethodWithGenericOutParametersTarget`2");
    }

    [Test]
    public void TypeParameters()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column:94,
            line:9,
            expected:"public bool TryRun(T1 t1, out T2 t2)",
            "LibraryThatReferencesLibrary.InSourceGotoDefinitionMethodWithGenericOutParametersTarget`2");
    }
}