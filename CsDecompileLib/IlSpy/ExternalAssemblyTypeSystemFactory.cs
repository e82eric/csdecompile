using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.IlSpy;

public class ExternalAssemblyTypeSystemFactory : IDecompilerTypeSystemFactory
{
    private readonly AssemblyResolverFactory _resolverFactory;

    public ExternalAssemblyTypeSystemFactory(
        AssemblyResolverFactory resolverFactory)
    {
        _resolverFactory = resolverFactory;
    }

    private PEFile OpenAssembly(string file)
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

    public DecompilerTypeSystem GetTypeSystem(string dllFilePath)
    {
        var peFile = OpenAssembly(dllFilePath);
        var assemblyResolver = _resolverFactory.GetAssemblyResolver(peFile);
        var result = new DecompilerTypeSystem(peFile, assemblyResolver);

        return result;
    }
}