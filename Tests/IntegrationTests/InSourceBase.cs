using NUnit.Framework;
using TryOmnisharpExtension;
using TryOmnisharpExtension.GotoDefinition;
using TryOmnisharpExtension.Roslyn;

namespace IntegrationTests;

public class InSourceGetSymbolInfoBase : InSourceBase
{
    protected void RequestAndAssertCorrectLine2(string filePath, int column, int line, string expected)
    {
        var response = ExecuteRequest<SymbolInfo>(Endpoints.SymbolInfo, filePath, column, line);
        AssertInSource(response, expected);
    }
    
    private static void AssertInSource(ResponsePacket<SymbolInfo> response, string expected)
    {
        Assert.AreEqual(response.Body.Result, expected);
    }
}

public class InSourceBase : TestBase
{
    protected void RequestAndAssertCorrectLine(string filePath, int column, int line, string expected)
    {
        RequestAndAssertCorrectLine(Endpoints.DecompileGotoDefinition, filePath, column, line, expected);
    }
    
    protected void RequestAndAssertCorrectLine(string command, string filePath, int column, int line, string expected)
    {
        var response = ExecuteRequest<DecompileGotoDefinitionResponse>(command, filePath, column, line);

        AssertInSource(response, expected);
    }

    protected static ResponsePacket<T> ExecuteRequest<T>(string command, string filePath, int column, int line)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = command,
            Arguments = new DecompiledLocationRequest
            {
                FileName = filePath,
                Column = column,
                IsDecompiled = false,
                Line = line
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, T>(request);
        Assert.True(response.Success);
        return response;
    }

    private static void AssertInSource(ResponsePacket<DecompileGotoDefinitionResponse> response, string expected)
    {
        Assert.AreEqual(response.Body.Location.Type, ResponseLocationType.SourceCode);
        AssertInSourceLocation(response.Body.Location, expected);
    }
    
    private static void AssertInSourceLocation(ResponseLocation location, string expected)
    {
        var lines = InSourceGetLines(location);
        var line = lines[location.Line - 1].Trim();

        Assert.AreEqual(expected, line);
    }
}