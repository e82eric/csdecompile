using System.Threading.Tasks;

namespace TryOmnisharpExtension.GotoDefinition;

class FileNotFoundCommand : INavigationCommand<DecompileGotoDefinitionResponse>
{
    private readonly string _filePath;

    public FileNotFoundCommand(string filePath)
    {
        _filePath = filePath;
    }
    public Task<DecompileGotoDefinitionResponse> Execute()
    {
        var result = new DecompileGotoDefinitionResponse()
        {
            ErrorDetails = new ErrorDetails()
            {
                Message = $"FILE_NOT_FOUND {_filePath}"
            }
        };
        return Task.FromResult(result);    
    }
}