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
        string kind,
        string shortName,
        string @namespace,
        Dictionary<string, object> expectProperties = null)
    {
        if (expectProperties == null)
        {
            expectProperties = new Dictionary<string, object>();
        }
        expectProperties.Add("Kind", kind);
        expectProperties.Add("ShortName", shortName);
        expectProperties.Add("Namespace", @namespace);
        var response = ExecuteRequest<SymbolInfo>(Endpoints.SymbolInfo, filePath, column, line);
        AssertItemsInProperties(response, expectProperties);
    }

    private static void AssertItemsInProperties(ResponsePacket<SymbolInfo> response, Dictionary<string, object> expectedProperties)
    {
        foreach (var key in expectedProperties.Keys)
        {
            var expected = expectedProperties[key];
            var actual = response.Body.Properties[key];
            Assert.AreEqual(expected, actual);
        }
    }
}