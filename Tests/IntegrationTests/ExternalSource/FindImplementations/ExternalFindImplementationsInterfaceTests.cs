using NUnit.Framework;
using TryOmnisharpExtension;

namespace IntegrationTests;

[TestFixture]
public class ExternalFindImplementationsInterfaceTests : ExternalFindImplementationsBase2
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
                (ResponseLocationType.Decompiled,
                    "public class ExternalFindImplementationsInterfaceInheritor : ExternalFindImplementationsInterface",
                    "ExternalFindImplementationsInterfaceInheritor")
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
                (ResponseLocationType.Decompiled,
                    "public void BasicMethod()",
                    "ExternalFindImplementationsInterfaceInheritor")
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
                (ResponseLocationType.Decompiled,
                    "public string BasicProperty { get; set; }",
                    "ExternalFindImplementationsInterfaceInheritor")
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
                (ResponseLocationType.Decompiled,
                    "public string BasicProperty { get; set; }",
                    "ExternalFindImplementationsInterfaceInheritor")
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
                (ResponseLocationType.Decompiled,
                    "public event EventHandler BasicEvent;",
                    "ExternalFindImplementationsInterfaceInheritor")
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
                (ResponseLocationType.Decompiled,
                    "public event EventHandler BasicEvent;",
                    "ExternalFindImplementationsInterfaceInheritor")
            });
    }
}