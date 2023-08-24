using NUnit.Framework;
using CsDecompileLib;

namespace IntegrationTests;

[TestFixture]
public class InSourceFindUsagesSymbolNotFoundAtLocationTests : InSourceBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath(
        "InSourceGotoDefinitionSymbolNotFoundAtLocationTarget.cs");
        
    [Test]
    public void GotoInSourceClassDefinition()
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileFindUsages,
            Arguments = new DecompiledLocationRequest
            {
                FileName = FilePath,
                Column = 10,
                Type = LocationType.SourceCode,
                Line = 4
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, LocationsResponse>(request);

        Assert.False(response.Success);
        Assert.AreEqual($"SYMBOL_NOT_FOUND_AT_LOCATION {FilePath}:4:10", response.Message);
    }
}