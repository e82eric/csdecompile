using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindImplementationsBaseClassTests : ExternalFindImplementationsBase2
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
                (LocationType.Decompiled,
                    "public class ExternalFindImplementationsBaseClassInheritor : ExternalFindImplementationsBaseClass",
                    "ExternalFindImplementationsBaseClassInheritor")
            });
    }
}