using System.Collections.Generic;
using CsDecompileLib;

namespace IntegrationTests;

public class FindImplementationsTestsBase
{
    protected void RequestAndAssertCorrectLine(
        string command,
        string filePath,
        int column,
        int line,
        IEnumerable<ExpectedImplementation> expected)
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = command,
            Arguments = new DecompiledLocationRequest
            {
                FileName = filePath,
                Column = column,
                Type = LocationType.SourceCode,
                Line = line
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, LocationsResponse>(request);

        ImplementationAsserts.AssertSame(response, expected);
    }
}