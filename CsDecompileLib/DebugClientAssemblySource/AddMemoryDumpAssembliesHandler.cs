using System.Threading.Tasks;

namespace CsDecompileLib.DebugClientAssemblySource;

public class AddMemoryDumpAssembliesHandler
    : HandlerBase<AddMemoryDumpAssembliesRequest, AddMemoryDumpAssembliesResponse>
{
    private readonly MemoryDumpLoader _memoryDumpLoader;

    public AddMemoryDumpAssembliesHandler(MemoryDumpLoader memoryDumpLoader)
    {
        _memoryDumpLoader = memoryDumpLoader;
    }
    public override Task<ResponsePacket<AddMemoryDumpAssembliesResponse>> Handle(AddMemoryDumpAssembliesRequest request)
    {
        _memoryDumpLoader.LoadDllsFromMemoryDump(request.MemoryDumpFilePath);
        var body = new AddMemoryDumpAssembliesResponse() { Success = true };
        return Task.FromResult(ResponsePacket.Ok(body));
    }
}