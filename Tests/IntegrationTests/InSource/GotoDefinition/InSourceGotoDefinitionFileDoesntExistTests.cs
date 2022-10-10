using NUnit.Framework;
using CsDecompileLib;
using CsDecompileLib.GotoDefinition;

namespace IntegrationTests;

[TestFixture]
public class InSourceGotoDefinitionFileDoesntExistTests : InSourceBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath("FileThatDoesntExist.cs");
        
    [Test]
    public void GotoInSourceClassDefinition()
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = new DecompiledLocationRequest
            {
                FileName = FilePath,
                Column = 58,
                Type = LocationType.SourceCode,
                Line = 3
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, GotoDefinitionResponse>(request);

        Assert.False(response.Success);
        Assert.AreEqual($"FILE_NOT_FOUND {FilePath}", response.Message);
    }
}