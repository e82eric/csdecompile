using System.Collections.Generic;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class DecompileAssemblyGetSymbolInfo : DecompileAssemblyTestBase
{
    [Test]
    public void Type()
    {
        var assemblyPath = TestHarness.GetLibraryThatReferencesLibraryAssemblyFilePath();
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            assemblyFilePath: assemblyPath,
            assemblyName: "LibraryThatReferencesLibrary",
            lineToFind:
            "ExternalSourceGetSymbolInfoTarget externalSourceGetSymbolInfoTarget = new ExternalSourceGetSymbolInfoTarget();",
            tokenToRequest: "ExternalSourceGetSymbolInfoTarget",
            new Dictionary<string, object>()
            {
                { "Kind", "TypeDefinition" },
                { "ShortName", "ExternalSourceGetSymbolInfoTarget" },
                { "Namespace", "LibraryThatJustReferencesFramework" }
            });
    }
}