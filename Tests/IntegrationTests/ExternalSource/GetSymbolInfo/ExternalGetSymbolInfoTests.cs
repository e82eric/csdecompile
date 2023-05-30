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
            "ExternalSourceGetSymbolInfoTarget externalSourceGetSymbolInfoTarget = new ExternalSourceGetSymbolInfoTarget\\(\\);",
            tokenToRequest: "(?<token>ExternalSourceGetSymbolInfoTarget) ",
            new Dictionary<string, object>()
            {
                { "FullName", "LibraryThatJustReferencesFramework.ExternalSourceGetSymbolInfoTarget" },
                { "IsStatic", "False" },
                { "IsSealed", "False" }
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
            "externalSourceGetSymbolInfoTarget.ExternalRun\\(\\);",
            tokenToRequest: "(?<token>ExternalRun)\\(\\);$",
            new Dictionary<string, object>
            {
                { "FullName", "LibraryThatJustReferencesFramework.ExternalSourceGetSymbolInfoTarget.ExternalRun" },
                { "ReturnType", "System.Void" },
                // { "Namespace", "LibraryThatJustReferencesFramework" }
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
            tokenToRequest: "(?<token>ExternalBasicProperty) =",
            new Dictionary<string, object>()
            {
                { "FullName", "LibraryThatJustReferencesFramework.ExternalSourceGetSymbolInfoTarget.ExternalBasicProperty" },
                { "ReturnType", "System.String" },
                // { "Namespace", "LibraryThatJustReferencesFramework" }
            });
    }
}