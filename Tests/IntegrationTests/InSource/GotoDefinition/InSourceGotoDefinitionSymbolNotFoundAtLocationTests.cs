using NUnit.Framework;
using CsDecompileLib;
using CsDecompileLib.GotoDefinition;

namespace IntegrationTests;

[TestFixture]
public class InSourceGotoDefinitionSymbolNotFoundAtLocationTests : InSourceBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath("InSourceGotoDefinitionSymbolNotFoundAtLocationTarget.cs");
        
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
                IsDecompiled = false,
                Line = 4
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, DecompileGotoDefinitionResponse>(request);

        Assert.False(response.Success);
        Assert.AreEqual($"SYMBOL_NOT_FOUND_AT_LOCATION {FilePath}:4:10", response.Message);
    }
}