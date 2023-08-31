using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class InSourceGotoDefinitionMethodTests : InSourceBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGotoDefinitionMethodCaller.cs");
    
    [Test]
    public void NoParameters()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column:54,
            line:9,
            expected: new ExpectedImplementation(
                LocationType.SourceCode,
                "public void BasicMethod()",
                "InSourceGotoDefinitionMethodTarget",
                "LibraryThatReferencesLibrary.InSourceGotoDefinitionMethodTarget"));
    }
    
    [Test]
    public void OneParameter()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column:54,
            line:10,
            expected: new ExpectedImplementation(
                LocationType.SourceCode,
                "public void BasicMethod(string param1)",
                "InSourceGotoDefinitionMethodTarget",
                "LibraryThatReferencesLibrary.InSourceGotoDefinitionMethodTarget"));
    }
    
    [Test]
    public void TwoParameters()
    {
        RequestAndAssertCorrectLine(
            filePath: FilePath,
            column:54,
            line:11,
            expected:new ExpectedImplementation(
                LocationType.SourceCode,
                "public void BasicMethod(string param1, string param2)",
                "InSourceGotoDefinitionMethodTarget",
                "LibraryThatReferencesLibrary.InSourceGotoDefinitionMethodTarget"));
    }
}