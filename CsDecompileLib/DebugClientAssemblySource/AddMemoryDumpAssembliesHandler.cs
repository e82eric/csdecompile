using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using Microsoft.Diagnostics.Runtime;

namespace CsDecompileLib.DebugClientAssemblySource;

public class AddMemoryDumpAssembliesHandler
    : HandlerBase<AddMemoryDumpAssembliesRequest, AddMemoryDumpAssembliesResponse>
{
    private readonly IDecompileWorkspace _decompileWorkspace;
    private readonly ClrMdDllExtractor _dllExtractor;

    public AddMemoryDumpAssembliesHandler(IDecompileWorkspace decompileWorkspace, ClrMdDllExtractor dllExtractor)
    {
        _decompileWorkspace = decompileWorkspace;
        _dllExtractor = dllExtractor;
    }
    public override Task<ResponsePacket<AddMemoryDumpAssembliesResponse>> Handle(AddMemoryDumpAssembliesRequest request)
    {
        using (DataTarget dataTarget = DataTarget.LoadDump(request.MemoryDumpFilePath))
        {
            var runtime = dataTarget.ClrVersions.Single().CreateRuntime();
            foreach (var module in runtime.AppDomains.FirstOrDefault()?.Modules)
            {
                if (module?.Name != null)
                {
                    PEFile peFile;
                    using (var memoryStream = new MemoryStream())
                    {
                        _dllExtractor.TryExtract(runtime.DataTarget.DataReader, module.ImageBase, memoryStream);

                        memoryStream.Seek(0, SeekOrigin.Begin);
                        peFile = new PEFile(
                            module.Name,
                            memoryStream,
                            streamOptions: PEStreamOptions.PrefetchEntireImage,
                            metadataOptions: new DecompilerSettings().ApplyWindowsRuntimeProjections
                                ? MetadataReaderOptions.ApplyWindowsRuntimeProjections
                                : MetadataReaderOptions.None);
                    }
                    _decompileWorkspace.AddPeFile(peFile);
                }
            }
        }

        var body = new AddMemoryDumpAssembliesResponse() { Success = true };
        return Task.FromResult(ResponsePacket.Ok(body));
    }
}