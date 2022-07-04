using NUnit.Framework;
using TryOmnisharpExtension;

namespace IntegrationTests;

[TestFixture]
public class InSourceFindImplementationsFileDoesntExistTests : InSourceBase
{
    private static string FilePath = TestHarness.GetLibraryThatReferencesLibraryFilePath("FileThatDoesntExist.cs");
        
    [Test]
    public void GotoInSourceClassDefinition()
    {
        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileFindImplementations,
            Arguments = new DecompiledLocationRequest
            {
                FileName = FilePath,
                Column = 58,
                IsDecompiled = false,
                Line = 3
            }
        };

        var response = TestHarness.IoClient
            .ExecuteCommand<DecompiledLocationRequest, FindImplementationsResponse>(request);

        Assert.False(response.Success);
        Assert.AreEqual($"FILE_NOT_FOUND {FilePath}", response.Message);
    }
}