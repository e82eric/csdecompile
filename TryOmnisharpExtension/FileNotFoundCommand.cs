using System.Threading.Tasks;
using TryOmnisharpExtension.GotoDefinition;

namespace TryOmnisharpExtension;

class FileNotFoundCommand<T> : INavigationCommand<T>
{
    private readonly string _filePath;

    public FileNotFoundCommand(string filePath)
    {
        _filePath = filePath;
    }
    public Task<ResponsePacket<T>> Execute()
    {
        var response = new ResponsePacket<T>
        {
            Success = false,
            Message = $"FILE_NOT_FOUND {_filePath}"
        };
        
        return Task.FromResult(response);    
    }
}