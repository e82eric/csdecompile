using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindImplementationsInterfaceTests : ExternalFindImplementationsBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "ExternalFindImplementationsInterfaceCaller.cs");
    [Test]
    public void FindFromInterfaceName()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 13,
            line: 10,
            expected: new []
            {
                new ExpectedImplementation(LocationType.Decompiled,
                    "public class ExternalFindImplementationsInterfaceInheritor : ExternalFindImplementationsInterface",
                    "ExternalFindImplementationsInterfaceInheritor",
                    "LibraryThatJustReferencesFramework.ExternalFindImplementationsInterfaceInheritor")
            });
    }
    
    [Test]
    public void FindFromInterfaceMethodName()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 15,
            line: 11,
            expected: new []
            {
                new ExpectedImplementation(LocationType.Decompiled,
                    "public void BasicMethod()",
                    "ExternalFindImplementationsInterfaceInheritor",
                    "LibraryThatJustReferencesFramework.ExternalFindImplementationsInterfaceInheritor")
            });
    }
    
    [Test]
    public void FindFromInterfacePropertyNameAsGetter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 23,
            line: 12,
            expected: new []
            {
                new ExpectedImplementation(LocationType.Decompiled,
                    "public string BasicProperty { get; set; }",
                    "ExternalFindImplementationsInterfaceInheritor",
                    "LibraryThatJustReferencesFramework.ExternalFindImplementationsInterfaceInheritor")
            });
    }
    
    [Test]
    public void FindFromInterfacePropertyNameAsSetter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 15,
            line: 12,
            expected: new []
            {
                new ExpectedImplementation(LocationType.Decompiled,
                    "public string BasicProperty { get; set; }",
                    "ExternalFindImplementationsInterfaceInheritor",
                    "LibraryThatJustReferencesFramework.ExternalFindImplementationsInterfaceInheritor")
            });
    }
    
    [Test]
    public void FindFromInterfaceEventSubscribe()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 15,
            line: 14,
            expected: new []
            {
                new ExpectedImplementation(LocationType.Decompiled,
                    "public event EventHandler BasicEvent;",
                    "ExternalFindImplementationsInterfaceInheritor",
                    "LibraryThatJustReferencesFramework.ExternalFindImplementationsInterfaceInheritor")
            });
    }
    
    [Test]
    public void FindFromInterfaceEventUnsubscribe()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 15,
            line: 15,
            expected: new []
            {
                new ExpectedImplementation(LocationType.Decompiled,
                    "public event EventHandler BasicEvent;",
                    "ExternalFindImplementationsInterfaceInheritor",
                    "LibraryThatJustReferencesFramework.ExternalFindImplementationsInterfaceInheritor")
            });
    }
}