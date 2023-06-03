using System.Collections.Generic;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class ExternalGetSymbolInfoGenericMethodTests : ExternalGetSymbolInfoTestBase
{
    private static string FilePath =
        TestHarness.GetLibraryThatReferencesLibraryFilePath("ExternalGetSymbolInfoGenericMethodUser.cs");

    [Test]
    public void Method1GenericParameter()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 54,
            line: 10,
            new Dictionary<string, object>
            {
                { "FullName", "LibraryThatJustReferencesFramework.ExternalGetSymbolInfoGenericMethodBase`1.Run" },
                { "ReturnType", "System.Void" },
            }, new Dictionary<string, string>()
            {
                { "t", "`0" },
            });
    }

    [Test]
    public void Method2GenericParameters()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 54,
            line: 11,
            new Dictionary<string, object>
            {
                { "FullName", "LibraryThatJustReferencesFramework.ExternalGetSymbolInfoGenericMethodBase`1.Run" },
                { "ReturnType", "System.Void" },
            },
            new Dictionary<string, string>()
            {
                { "t", "`0" },
                { "t2", "`0" }
            });
    }

    [Test]
    public void Method2ParametersOneNotGeneric()
    {
        SendRequestAndAssertLine(
            filePath: FilePath,
            column: 54,
            line: 12,
            new Dictionary<string, object>
            {
                { "FullName", "LibraryThatJustReferencesFramework.ExternalGetSymbolInfoGenericMethodBase`1.Run" },
                { "ReturnType", "System.Void" },
            }, new Dictionary<string, string>
            {
                { "t", "`0" },
                { "s", "System.String" }
            });
    }
}