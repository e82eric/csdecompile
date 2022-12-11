using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class DecompileAssemblyAndFindUsagesTests : DecompileAssemblyTestBase
{
    [Test]
    public void Method()
    {
        var assemblyPath = TestHarness.GetLibraryThatReferencesLibraryAssemblyFilePath();
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            Endpoints.DecompileFindUsages,
            assemblyFilePath: assemblyPath,
            assemblyName: "LibraryThatReferencesLibrary",
            lineToFind: "new ExternalFindUsagesMethodTarget().ExternalBasicMethod();",
            tokenToRequest: "ExternalBasicMethod",
            expected: new[]
            {
                (LocationType.SourceCode,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod();",
                    "ExternalFindUsagesMethodCaller"),
                (LocationType.Decompiled,
                    "new ExternalFindUsagesMethodTarget().ExternalBasicMethod();",
                    "ExternalFindUsagesMethodUser"),
                (LocationType.Decompiled,
                    "Run1(new ExternalFindUsagesMethodTarget().ExternalBasicMethod);",
                    "ExternalFindUsagesMethodUser"),
            });
    }
}