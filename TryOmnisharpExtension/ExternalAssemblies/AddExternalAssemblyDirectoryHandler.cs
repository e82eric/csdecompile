using System.IO;
using System.Threading.Tasks;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.ExternalAssemblies;

public class AddExternalAssemblyDirectoryHandler
{
    private readonly IDecompileWorkspace _decompileWorkspace;

    public AddExternalAssemblyDirectoryHandler(
        IDecompileWorkspace decompileWorkspace)
    {
        _decompileWorkspace = decompileWorkspace;
    }
    
    public Task<AddExternalAssemblyDirectoryResponse> Handle(AddExternalAssemblyDirectoryRequest request)
    {
        var directoryFileInfo = new DirectoryInfo(request.DirectoryFilePath);
        _decompileWorkspace.LoadDllsInDirectory(directoryFileInfo);
        var response = new AddExternalAssemblyDirectoryResponse() { Success = true };
        return Task.FromResult(response);
    }
}