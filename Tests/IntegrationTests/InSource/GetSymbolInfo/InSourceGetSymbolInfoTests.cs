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
            new Dictionary<string, string>()
            {
                {"AssemblyPath", "InSourceGetSymbolInfoTarget"},
                {"Namespace", "LibraryThatReferencesLibrary"},
                {"DisplayName", "InSourceGetSymbolInfoTarget"},
                {"Kind", "NamedType"}
            },
            new Dictionary<string, object>()
            {
                {"FullName", "InSourceGetSymbolInfoTarget"},
            });
    }
    
    [Test]
    public void Constructor()
    {
        RequestAndAssertContainsProperties(
            filePath: FilePath,
            column: 51,
            line: 7,
            new Dictionary<string, string>()
            {
                {"AssemblyPath", "InSourceGetSymbolInfoTarget.InSourceGetSymbolInfoTarget()"},
                {"Namespace", "LibraryThatReferencesLibrary"},
                {"DisplayName", ".ctor"},
                {"Kind", "Method"}
            },
            new Dictionary<string, object>()
            {
                {"FullName", "InSourceGetSymbolInfoTarget.InSourceGetSymbolInfoTarget()"},
                {"ReturnType", "void"},
            });
    }
    
    [Test]
    public void Method()
    {
        RequestAndAssertContainsProperties(
            filePath: FilePath,
            column:17,
            line:8,
            new Dictionary<string, string>()
            {
                {"AssemblyPath", "void InSourceGetSymbolInfoTarget.Run()"},
                {"Namespace", "LibraryThatReferencesLibrary"},
                {"DisplayName", "Run"},
                {"Kind", "Method"}
            },
            new Dictionary<string, object>()
            {
                {"FullName", "void InSourceGetSymbolInfoTarget.Run()"},
                {"ReturnType", "void"},
            });
    }
    
    [Test]
    public void Property()
    {
        RequestAndAssertContainsProperties(
            filePath: FilePath,
            column:17,
            line:9,
            new Dictionary<string, string>()
            {
                {"AssemblyPath", "string InSourceGetSymbolInfoTarget.BasicProperty"},
                {"Namespace", "LibraryThatReferencesLibrary"},
                {"DisplayName", "BasicProperty"},
                {"Kind", "Property"}
            },
            new Dictionary<string, object>()
            {
                {"FullName", "string InSourceGetSymbolInfoTarget.BasicProperty"},
                {"Type", "string"},
            });
    }
}