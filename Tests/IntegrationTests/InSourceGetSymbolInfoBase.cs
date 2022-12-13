using System.Collections.Generic;
using CsDecompileLib;
using CsDecompileLib.Roslyn;
using NUnit.Framework;

namespace IntegrationTests;

public class InSourceGetSymbolInfoBase : InSourceBase
{
    protected void RequestAndAssertContainsProperties(
        string filePath,
        int column,
        int line,
        Dictionary<string, string> expectedHeaderProperties = null,
        Dictionary<string, object> expectProperties = null)
    {
        if (expectProperties == null)
        {
            expectProperties = new Dictionary<string, object>();
        }
        var response = ExecuteRequest<SymbolInfo>(Endpoints.SymbolInfo, filePath, column, line);
        AssertItemsInHeaderProperties(response.Body.HeaderProperties, expectedHeaderProperties);
        AssertItemsInProperties(response.Body.Properties, expectProperties);
    }

    private static void AssertItemsInProperties(Dictionary<string, object> actualProperties, Dictionary<string, object> expectedProperties)
    {
        foreach (var key in expectedProperties.Keys)
        {
            var expected = expectedProperties[key];
            var actual = actualProperties[key];
            Assert.AreEqual(expected, actual);
        }
    }
    
    private static void AssertItemsInHeaderProperties(Dictionary<string, string> actualProperties, Dictionary<string, string> expectedProperties)
    {
        foreach (var key in expectedProperties.Keys)
        {
            var expected = expectedProperties[key];
            var actual = actualProperties[key];
            Assert.AreEqual(expected, actual);
        }
    }
}