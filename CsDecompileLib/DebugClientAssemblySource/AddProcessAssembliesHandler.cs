using System.Threading.Tasks;

namespace CsDecompileLib.DebugClientAssemblySource;

public class AddProcessAssembliesHandler
    : HandlerBase<AddProcessAssembliesRequest, AddMemoryDumpAssembliesResponse>
{
    private readonly MemoryDumpLoader _memoryDumpLoader;

    public AddProcessAssembliesHandler(MemoryDumpLoader memoryDumpLoader)
    {
        _memoryDumpLoader = memoryDumpLoader;
    }
    public override Task<ResponsePacket<AddMemoryDumpAssembliesResponse>> Handle(AddProcessAssembliesRequest request)
    {
        _memoryDumpLoader.LoadAssembliesFromProcess(request.ProcessId, request.Suspend);
        var body = new AddMemoryDumpAssembliesResponse { Success = true };
        return Task.FromResult(ResponsePacket.Ok(body));
    }
}