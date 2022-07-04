using System.Collections.Generic;
using TryOmnisharpExtension;

namespace IntegrationTests;

public class ExternalFindUsagesTestBase : ExternalFindImplementationsBase
{
    protected void SendRequestAndAssertLine(
        string filePath,
        int column,
        int line,
        IEnumerable<(ResponseLocationType type, string value)> expected)
    {
        SendRequestAndAssertLine(
            Endpoints.DecompileFindUsages,
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
            Endpoints.DecompileFindUsages,
            filePath,
            lineToFind,
            tokenToFind,
            column,
            line,
            expected);
    }
}