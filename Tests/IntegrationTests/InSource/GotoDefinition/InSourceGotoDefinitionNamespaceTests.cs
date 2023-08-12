using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class InSourceGotoDefinitionNamespaceTests : InSourceBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGotoDefinitionNamespaceUser.cs");

    [Test]
    public void Test1()
    {
        RequestAndAssertCorrectLocations(
            filePath: FilePath,
            column: 7,
            line: 1,
            expected: new[]
            {
                (LocationType.SourceCode, "public class InSourceClass1 {}", "InSourceClass1"),
                (LocationType.SourceCode, "public class InSourceClass2 {}", "InSourceClass2"),
                (LocationType.SourceCode, "public class InSourceClass3 {}", "InSourceClass3")
            });
    }

    [Test]
    public void Test2()
    {
        RequestAndAssertCorrectLocations(
            filePath: FilePath,
            column: 7,
            line: 2,
            expected: new[]
            {
                (LocationType.SourceCode, "public class InSourceClass1 {}", "InSourceClass1"),
                (LocationType.SourceCode, "public class InSourceClass2 {}", "InSourceClass2"),
                (LocationType.SourceCode, "public class InSourceClass3 {}", "InSourceClass3")
            });
    }

    [Test]
    public void Test3()
    {
        RequestAndAssertCorrectLocations(
            filePath: FilePath,
            column: 45,
            line: 2,
            expected: new[]
            {
                (LocationType.SourceCode, "public class SubInSourceClass1 {}", "SubInSourceClass1"),
                (LocationType.SourceCode, "public class SubInSourceClass2 {}", "SubInSourceClass2"),
                (LocationType.SourceCode, "public class SubInSourceClass3 {}", "SubInSourceClass3")
            });
    }
}