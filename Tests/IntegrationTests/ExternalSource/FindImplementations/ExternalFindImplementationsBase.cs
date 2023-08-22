using System.Collections.Generic;
using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

public class ExternalFindImplementationsBase : ExternalTestBase
{
    protected void SendRequestAndAssertLine(
        string filePath,
        int column,
        int line,
        IEnumerable<ExpectedImplementation> expected)
    {
        SendRequestAndAssertLine(
            Endpoints.DecompileFindImplementations,
            filePath,
            column,
            line,
            expected);
    }
    protected void SendRequestAndAssertLine(
        string command,
        string filePath,
        int column,
        int line,
        IEnumerable<ExpectedImplementation> expected)
    {
        var requestArguments = new DecompiledLocationRequest
        {
            FileName = filePath,
            Column = column,
            Type = LocationType.SourceCode,
            Line = line,
            AssemblyName = null,
            AssemblyFilePath = null
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
        IEnumerable<ExpectedImplementation> expected)
    {
        DecompiledLocationRequest definitionRequestArguments = GotoDefinitionAndCreateRequestForToken(
            filePath,
            column,
            line,
            lineToFind,
            tokenToFind);
        
        SendRequestAndAssertLine(command, expected, definitionRequestArguments);
    }
    
    protected void SendRequestAndAssertNumberOfImplementations(
        string command,
        string filePath,
        string lineToFind,
        string tokenToFind,
        string lineToFind2,
        string tokenPatternToFind2,
        int column,
        int line,
        int numberOfImplementations)
    {
        DecompiledLocationRequest definitionRequestArguments = GotoDefinitionAndCreateRequestForToken(
            filePath,
            column,
            line,
            lineToFind,
            tokenToFind,
            lineToFind2,
            tokenPatternToFind2);
        
        SendRequestAndAssertNumberOfImplementations(command, numberOfImplementations, definitionRequestArguments);
    }

    private static void SendRequestAndAssertLine(
        string command,
        IEnumerable<ExpectedImplementation> expected,
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

        ImplementationAsserts.AssertSame(response, expected);
    }
    
    private static void SendRequestAndAssertNumberOfImplementations(
        string command,
        int expected,
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
        Assert.AreEqual(expected, response.Body.Implementations.Count);
    }
}