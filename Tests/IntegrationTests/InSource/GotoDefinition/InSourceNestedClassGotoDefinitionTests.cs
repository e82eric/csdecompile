using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceNestedClassGotoDefinitionTests : InSourceBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "InSourceNestedClassGotoDefinitionUser.cs");

    [Test]
    public void InnerClass()
    {
        RequestAndAssertCorrectLine(
            filePath:FilePath,
            column:53,
            line: 7,
            new ExpectedImplementation(
                LocationType.SourceCode,
                "public class InnerClass",
                null,
                "LibraryThatReferencesLibrary.InSourceNestedClassGotoDefinitionTarget"));
    }

    [Test]
    public void ParentClass()
    {
        RequestAndAssertCorrectLine(
            filePath:FilePath,
            column:13,
            line: 7,
            new ExpectedImplementation(
                LocationType.SourceCode,
                "public class InSourceNestedClassGotoDefinitionTarget",
                null,
                "LibraryThatReferencesLibrary.InSourceNestedClassGotoDefinitionTarget"));
    }
}