using System.Collections.Generic;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGetSymbolInfoTests : ExternalGetSymbolInfoTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalSourceGetSymbolInfoCaller.cs");
    
    [Test]
    public void Type()
    {
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            filePath: FilePath,
            column: 13,
            line: 9,
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

    [Test]
    public void Method()
    {
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            filePath: FilePath,
            column: 13,
            line: 9,
            lineToFind:
            "externalSourceGetSymbolInfoTarget.ExternalRun();",
            tokenToRequest: "ExternalRun",
            new Dictionary<string, object>
            {
                { "Kind", "Method" },
                { "ShortName", "ExternalRun" },
                { "Namespace", "LibraryThatJustReferencesFramework" }
            });
    }
    
    [Test]
    public void Property()
    {
        SendRequestFindLocationInDecompiledClassRequestAgainAndAssertLine(
            filePath: FilePath,
            column: 13,
            line: 9,
            lineToFind:
            "externalSourceGetSymbolInfoTarget.ExternalBasicProperty = string.Empty;",
            tokenToRequest: "ExternalBasicProperty",
            new Dictionary<string, object>()
            {
                { "Kind", "Property" },
                { "ShortName", "ExternalBasicProperty" },
                { "Namespace", "LibraryThatJustReferencesFramework" }
            });
    }
}