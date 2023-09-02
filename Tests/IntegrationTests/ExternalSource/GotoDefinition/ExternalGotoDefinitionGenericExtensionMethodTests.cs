using NUnit.Framework;

namespace IntegrationTests;

public class ExternalGotoDefinitionGenericExtensionMethodTests : ExternalGotoDefinitionTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGotoDefinitionGenericExtensionMethodUser.cs");

    [Test]
    public void TypeParameterSpecified()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 15,
            line: 9,
            expected: "public static TSource[] Run<TSource>(this ExternalGotoDefinitionGenericExtensionMethodTarget<TSource> source)",
            "LibraryThatJustReferencesFramework.ExternalGotoDefinitionGenericExtensionMethodTargetExtensions");
    }
}