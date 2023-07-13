using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionDotNetCoreFrameworkClassThroughNugetPackageTests : ExternalGotoDefinitionTestBase
{
    public ExternalGotoDefinitionDotNetCoreFrameworkClassThroughNugetPackageTests() : base(TestHarness.DotNetCoreIoClient)
    {
    }
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesFirlyFilePath("Class1.cs");
    [Test]
    public void GotoTypeDefintionForCoreFrameworkCodeThatIsNotDirectlyReferencedByProjects2()
    {
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            FilePath,
            21,
            9,
            "\\[DataContract\\]",
            "\\[(?<token>DataContract)\\]",
            "public sealed class DataContractAttribute : Attribute",
            " (?<token>Attribute)$",
            lineToFind3: "\\[Serializable\\]",
            line3TokenRegex: "\\[(?<token>Serializable)\\]",
            "public sealed class SerializableAttribute : Attribute",
            containingTypeFullName:"System.SerializableAttribute");
    }
}