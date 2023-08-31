using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceGotoDefinitionMethodWithGenericParametersTests : InSourceBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGotoDefinitionMethodWithGenericParametersUser.cs");

    [Test]
    public void Parameters()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column:91,
            line:7,
            expected: new ExpectedImplementation(
                LocationType.SourceCode,
                "public void TryRun(T1 t1, T2 t2)",
                "InSourceGotoDefinitionMethodWithGenericParametersTarget",
                "LibraryThatReferencesLibrary.InSourceGotoDefinitionMethodWithGenericParametersTarget`2"));
    }
}