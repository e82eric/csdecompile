using System.Collections.Generic;
using CsDecompileLib;

namespace IntegrationTests;

public class ExternalFindUsagesTestBase : ExternalFindImplementationsBase
{
    protected void SendRequestAndAssertLine(
        string filePath,
        int column,
        int line,
        IEnumerable<(LocationType type, string value, string shortTypeName)> expected)
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
        IEnumerable<(LocationType type, string value, string shortTypeName)> expected)
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
    
    protected void SendRequestAndAssertNumberOfImplementations(
        string filePath,
        string lineToFind,
        string tokenToFind,
        string lineToFind2,
        string tokenToFind2,
        int column,
        int line,
        int numberOfImplementations)
    {
        SendRequestAndAssertNumberOfImplementations(
            Endpoints.DecompileFindUsages,
            filePath,
            lineToFind,
            tokenToFind,
            lineToFind2,
            tokenToFind2,
            column,
            line,
            numberOfImplementations);
    }
}