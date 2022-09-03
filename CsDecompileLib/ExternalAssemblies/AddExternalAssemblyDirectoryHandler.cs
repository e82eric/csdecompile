using System.IO;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;

namespace CsDecompileLib.ExternalAssemblies;

public class AddExternalAssemblyDirectoryHandler
    : HandlerBase<AddExternalAssemblyDirectoryRequest, AddExternalAssemblyDirectoryResponse>
{
    private readonly IDecompileWorkspace _decompileWorkspace;

    public AddExternalAssemblyDirectoryHandler(
        IDecompileWorkspace decompileWorkspace)
    {
        _decompileWorkspace = decompileWorkspace;
    }
    
    public override Task<ResponsePacket<AddExternalAssemblyDirectoryResponse>> Handle(AddExternalAssemblyDirectoryRequest request)
    {
        var directoryFileInfo = new DirectoryInfo(request.DirectoryFilePath);
        _decompileWorkspace.LoadDllsInDirectory(directoryFileInfo);
        var body = new AddExternalAssemblyDirectoryResponse { Success = true };
        var result = ResponsePacket.Ok(body);
        return Task.FromResult(result);
    }
}