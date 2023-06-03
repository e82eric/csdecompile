using System.Collections.Generic;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGetSymbolFrameworkGenericMethodUserTests : ExternalGetSymbolInfoTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGetSymbolFrameworkGenericMethodUser.cs");
    
    [Test]
    public void Method()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 71,
            line: 10,
            new Dictionary<string, object>
            {
                { "FullName", "System.Collections.Generic.List`1.Add" },
                { "ReturnType", "System.Void" },
            },
            new Dictionary<string, string>()
            {
                { "item", "`0"}
            });
    }
}