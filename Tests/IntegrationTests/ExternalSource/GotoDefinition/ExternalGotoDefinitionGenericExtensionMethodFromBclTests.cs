using NUnit.Framework;

namespace IntegrationTests;

public class ExternalGotoDefinitionGenericExtensionMethodFromBclTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionGenericExtensionMethodFromBclUser.cs");

    [Test]
    public void TypeParameterSpecified()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 22,
            line: 11,
            expected: "public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)",
            "System.Linq.Enumerable");
    }
    [Test]
    public void DirectlyUsingCollection()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 15,
            line: 12,
            expected: "public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)",
            "System.Linq.Enumerable");
    }
}