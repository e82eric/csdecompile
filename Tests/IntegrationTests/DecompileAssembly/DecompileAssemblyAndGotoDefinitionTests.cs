using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class DecompileAssemblyAndGotoDefinitionTests : DecompileAssemblyTestBase
{
    [Test]
    public void Type()
    {
        var assemblyPath = TestHarness.GetLibraryThatReferencesLibraryAssemblyFilePath();
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            assemblyFilePath: assemblyPath,
            assemblyName: "LibraryThatReferencesLibrary",
            lineToFind: "DecompileAssemblyTypeTarget decompileAssemblyTypeTarget = null;",
            tokenToRequest:"DecompileAssemblyTypeTarget",
            expected:"public class DecompileAssemblyTypeTarget");
    }
}