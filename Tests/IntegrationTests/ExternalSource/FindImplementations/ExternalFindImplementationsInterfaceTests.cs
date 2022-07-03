using NUnit.Framework;

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
                "public class ExternalFindImplementationsInterfaceInheritor : ExternalFindImplementationsInterface"
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
                "public void BasicMethod()"
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
                "public string BasicProperty { get; set; }"
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
                "public string BasicProperty { get; set; }"
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
                "public event EventHandler BasicEvent;"
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
                "public event EventHandler BasicEvent;"
            });
    }
}