using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension;

public class ExternalAssemblyTypeSystemFactory : IDecompilerTypeSystemFactory
{
    private readonly AssemblyResolverFactory _resolverFactory;

    public ExternalAssemblyTypeSystemFactory(
        AssemblyResolverFactory resolverFactory)
    {
        _resolverFactory = resolverFactory;
    }

    private async Task<PEFile> OpenAssembly(string file)
    {
        using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
        {
            var result = LoadAssembly(fileStream, PEStreamOptions.PrefetchEntireImage, file);
            return result;
        }
    }
        
    private PEFile LoadAssembly(Stream stream, PEStreamOptions streamOptions, string fileName)
    {
        var options = MetadataReaderOptions.ApplyWindowsRuntimeProjections;

        PEFile module = new PEFile(fileName, stream, streamOptions, metadataOptions: options);

        return module;
    }

    public async Task<DecompilerTypeSystem> GetTypeSystem(string dllFilePath)
    {
        var peFile = await OpenAssembly(dllFilePath);
        var assemblyResolver = await _resolverFactory.GetAssemblyResolver(peFile);
        var result = new DecompilerTypeSystem(peFile, assemblyResolver);

        return result;
    }
}