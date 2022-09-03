using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CsDecompileLib;
using CsDecompileLib.GetSource;

namespace IntegrationTests;

public class ExternalFindImplementationsBase : ExternalTestBase
{
    protected void SendRequestAndAssertLine(
        string command,
        string filePath,
        int column,
        int line,
        IEnumerable<(ResponseLocationType type, string value, string shortTypeName)> expected)
    {
        var requestArguments = new DecompiledLocationRequest
        {
            FileName = filePath,
            Column = column,
            IsDecompiled = false,
            Line = line
        };

        SendRequestAndAssertLine(command, expected, requestArguments);
    }

    protected void SendRequestAndAssertLine(
        string command,
        string filePath,
        string lineToFind,
        string tokenToFind,
        int column,
        int line,
        IEnumerable<(ResponseLocationType type, string value, string shortTypeName)> expected)
    {
        DecompiledLocationRequest definitionRequestArguments = GotoDefintionAndCreateRequestForToken(
            filePath,
            column,
            line,
            lineToFind,
            tokenToFind);
        
        SendRequestAndAssertLine(command,  expected, definitionRequestArguments);
    }
    
    private static void SendRequestAndAssertLine(
        string command,
        IEnumerable<(ResponseLocationType type, string value, string shortTypeName)> expected,
        DecompiledLocationRequest requestArguments)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = command,
            Arguments = requestArguments
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, FindImplementationsResponse>(request);

        Assert.True(response.Success);
        Assert.AreEqual(expected.Count(), response.Body.Implementations.Count());

        foreach (var implementation in response.Body.Implementations)
        {
            string[] lines = null;
            switch (implementation.Type)
            {
                case ResponseLocationType.Decompiled:
                    lines = ExternalGetLines(implementation);
                    break;
                case ResponseLocationType.SourceCode:
                    lines = InSourceGetLines(implementation);
                    break;
            }

            var sourceLine = lines[implementation.Line - 1].Trim();

            var foundExpected = expected.FirstOrDefault(e =>
                e.type == implementation.Type &&
                e.value.Contains(sourceLine));
            Assert.NotNull(foundExpected);
            Assert.AreEqual(foundExpected.type, implementation.Type);
            Assert.AreEqual(implementation.SourceText, sourceLine);
            Assert.AreEqual(foundExpected.shortTypeName, implementation.ContainingTypeShortName);
        }
    }

    private static string[] ExternalGetLines(ResponseLocation implementation)
    {
        var decompileInfo = (DecompileInfo)implementation;

        var sourceRequest = new CommandPacket<DecompiledSourceRequest>
        {
            Command = Endpoints.DecompiledSource,
            Arguments = new DecompiledSourceRequest
            {
                AssemblyFilePath = decompileInfo.AssemblyFilePath,
                ContainingTypeFullName = decompileInfo.ContainingTypeFullName,
                Column = decompileInfo.Column,
                Line = decompileInfo.Line
            }
        };

        var sourceResponse = TestHarness.IoClient
            .ExecuteCommand<DecompiledSourceRequest, DecompiledSourceResponse>(sourceRequest);

        var lines = GetLines(sourceResponse.Body.SourceText);
        return lines;
    }
}