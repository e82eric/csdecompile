using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGotoDefinitionDotNetCoreFrameworkClassTests : ExternalGotoDefinitionTestBase
{
    public ExternalGotoDefinitionDotNetCoreFrameworkClassTests() : base(TestHarness.DotNetCoreIoClient)
    {
    }
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesCoreLibraryFilePath("DataContractUser.cs");
    [Test]
    public void GotoTypeDefintionForCoreFrameworkCodeThatIsNotDirectlyReferencedByProjects()
    {
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            FilePath,
            13,
            9,
            "\\[DataContract\\]",
            "\\[(?<token>DataContract)\\]",
            "public sealed class DataContractAttribute : Attribute",
            ": (?<token>Attribute)$",
            "public abstract class Attribute",
            containingTypeFullName:"System.Attribute");
    }
}