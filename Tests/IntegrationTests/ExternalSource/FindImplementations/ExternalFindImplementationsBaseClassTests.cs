using NUnit.Framework;
using CsDecompileLib;

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
                new ExpectedImplementation(LocationType.Decompiled,
                    "public class ExternalFindImplementationsBaseClassInheritor : ExternalFindImplementationsBaseClass",
                    "ExternalFindImplementationsBaseClassInheritor",
                    "LibraryThatJustReferencesFramework.ExternalFindImplementationsBaseClassInheritor")
            });
    }
}