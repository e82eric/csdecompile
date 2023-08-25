using CsDecompileLib;
using CsDecompileLib.GotoDefinition;
using NUnit.Framework;

namespace IntegrationTests.InSource.GotoDefinition;

[TestFixture]
public class InSourceGotoDefinitionSymbolNotFoundAtLocationTests : InSourceBase
{
    private static readonly string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGotoDefinitionSymbolNotFoundAtLocationTarget.cs");
        
    [Test]
    public void GotoInSourceClassDefinition()
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = new DecompiledLocationRequest
            {
                FileName = FilePath,
                Column = 10,
                Type = LocationType.SourceCode,
                Line = 4
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, DecompileAssemlbyResponse>(request);

        Assert.False(response.Success);
        Assert.AreEqual($"SYMBOL_NOT_FOUND_AT_LOCATION {FilePath}:4:10", response.Message);
    }
}