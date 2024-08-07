using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using Microsoft.Diagnostics.Runtime;

namespace CsDecompileLib.DebugClientAssemblySource;

public class MemoryDumpLoader
{
    private readonly IDecompileWorkspace _decompileWorkspace;
    private readonly ClrMdDllExtractor _dllExtractor;
    private readonly DataTargetProvider _dataTargetProvider;

    public MemoryDumpLoader(
        IDecompileWorkspace decompileWorkspace,
        ClrMdDllExtractor dllExtractor,
        DataTargetProvider dataTargetProvider)
    {
        _decompileWorkspace = decompileWorkspace;
        _dllExtractor = dllExtractor;
        _dataTargetProvider = dataTargetProvider;
    }

    public void LoadAssembliesFromProcess(int processId, bool suspend)
    {
        _dataTargetProvider.SetFromProcess(processId, suspend);
        Load(_dataTargetProvider.Get());
    }

    public void LoadDllsFromMemoryDump(string filePath)
    {
        _dataTargetProvider.SetFromMemoryDump(filePath);
        Load(_dataTargetProvider.Get());
    }

    private void Load(DataTarget dataTarget)
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
}