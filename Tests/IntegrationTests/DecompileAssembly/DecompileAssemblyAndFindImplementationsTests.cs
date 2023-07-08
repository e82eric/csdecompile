using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class DecompileAssemblyAndFindImplementationsTests : DecompileAssemblyTestBase
{
    [Test]
    public void GotoExternalClassDefinition()
    {
        var assemblyPath = TestHarness.GetLibraryThatReferencesLibraryAssemblyFilePath();
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            Endpoints.DecompileFindImplementations,
            assemblyFilePath: assemblyPath,
            assemblyName: "",
            "public class InSourceFindImplementationsBaseClass",
            "InSourceFindImplementationsBaseClass",
            expected: new []
            {
                (LocationType.SourceCode,
                    "public class InSourceFindImplementationsBaseClassInheritor : InSourceFindImplementationsBaseClass",
                    "InSourceFindImplementationsBaseClassInheritor"),
                (LocationType.SourceCode,
                    "public class InSourceFindImplementationsBaseClass",
                    "InSourceFindImplementationsBaseClass"),
                (LocationType.Decompiled,
                    "public class InSourceFindImplementationsBaseClassInheritor : InSourceFindImplementationsBaseClass",
                    "InSourceFindImplementationsBaseClassInheritor")
            });
    }
}