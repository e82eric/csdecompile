using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindImplementationsBaseClassTests : ExternalFindImplementationsBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindImplementationsBaseClassCaller.cs");
    [Test]
    public void GotoExternalClassDefinition()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 13,
            line: 9,
            expected: new []
            {
                "public class ExternalFindImplementationsBaseClassInheritor : ExternalFindImplementationsBaseClass"
            });
    }
}