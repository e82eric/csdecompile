using System.Collections.Generic;
using TryOmnisharpExtension;

namespace IntegrationTests;

public class ExternalFindImplementationsBase2 : ExternalFindImplementationsBase
{
    protected void SendRequestAndAssertLine(
        string filePath,
        int column,
        int line,
        IEnumerable<(ResponseLocationType type, string value)> expected)
    {
        SendRequestAndAssertLine(
            Endpoints.DecompileFindImplementations,
            filePath,
            column,
            line,
            expected);
    }
    
    protected void SendRequestAndAssertLine(
        string filePath,
        string lineToFind,
        string tokenToFind,
        int column,
        int line,
        IEnumerable<(ResponseLocationType type, string value)> expected)
    {
        SendRequestAndAssertLine(
            Endpoints.DecompileFindImplementations,
            filePath,
            lineToFind,
            tokenToFind,
            column,
            line,
            expected);
    }
}