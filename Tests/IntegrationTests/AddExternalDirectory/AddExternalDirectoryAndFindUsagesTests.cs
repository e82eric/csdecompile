using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class AddExternalDirectoryAndFindUsagesTests : AddExternalDirectoryTestBase
{
    [Test]
    public void Method()
    {
        var assemblyPath = TestHarness.GetLibraryThatReferencesLibraryAssemblyBinDir();
        SearchForTokenInDecompiledTypeAndAssertUsages(
            assemblyPath,
            "ExternalFindUsagesMethodOfInterfaceImplementation",
            "void ExternalFindUsagesMethodOfInterfaceInterface.Run()",
            "Run()",
            expected: new []
            {
                (LocationType.Decompiled,
                    "_externalFindUsagesMethodOfInterfaceInterface2.Run();",
                    "ExternalFindUsagesMethodOfInterfaceUser2"),
                (LocationType.Decompiled,
                    "_externalFindUsagesMethodOfInterfaceInterface1.Run();",
                    "ExternalFindUsagesMethodOfInterfaceUser1"),
            });
    }
}