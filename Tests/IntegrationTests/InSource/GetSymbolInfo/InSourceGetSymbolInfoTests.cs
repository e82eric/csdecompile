using System.Collections.Generic;
using NUnit.Framework;

namespace IntegrationTests.InSource.GetSymbolInfo;

[TestFixture]
public class InSourceGetSymbolInfoTests : InSourceGetSymbolInfoBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGetSymbolInfoCaller.cs");
    
    [Test]
    public void Type()
    {
        RequestAndAssertContainsProperties(
            filePath: FilePath,
            column: 23,
            line: 7,
            "NamedType",
            "InSourceGetSymbolInfoTarget",
            "LibraryThatReferencesLibrary");
    }
    
    [Test]
    public void Constructor()
    {
        RequestAndAssertContainsProperties(
            filePath: FilePath,
            column: 51,
            line: 7,
            "Method",
            ".ctor",
            "LibraryThatReferencesLibrary");
    }
    
    [Test]
    public void Method()
    {
        RequestAndAssertContainsProperties(
            filePath: FilePath,
            column:17,
            line:8,
            "Method",
            "Run",
            "LibraryThatReferencesLibrary",
            new Dictionary<string, object>()
            {
                {"ReturnType", "void"}
            });
    }
    
    [Test]
    public void Property()
    {
        RequestAndAssertContainsProperties(
            filePath: FilePath,
            column:17,
            line:9,
            "Property",
            "BasicProperty",
            "LibraryThatReferencesLibrary");
    }
}