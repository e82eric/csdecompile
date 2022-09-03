using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using ICSharpCode.Decompiler.Metadata;

namespace CsDecompileLib.IlSpy
{
    public class AssemblyResolverFactory
    {
        private readonly PeFileCache _peFileCache;

        public AssemblyResolverFactory(PeFileCache peFileCache)
        {
            _peFileCache = peFileCache;
        }
        
        public IAssemblyResolver GetAssemblyResolver(PEFile peFile)
        {
            var targetFrameworkId = peFile.DetectTargetFrameworkId();
            var universalResolver = GetUniversalResolver(targetFrameworkId, peFile);
            return new MyAssemblyResolver(_peFileCache, targetFrameworkId, universalResolver);
        }
        
        private UniversalAssemblyResolver GetUniversalResolver(string targetFrameworkId, PEFile peFile)
        {
            var runtimePack = peFile.DetectRuntimePack();  

            var readerOptions = MetadataReaderOptions.ApplyWindowsRuntimeProjections;

            var rootedPath = Path.IsPathRooted(peFile.FileName) ? peFile.FileName : null;

            return new UniversalAssemblyResolver(
                rootedPath,
                throwOnError: false,
                targetFrameworkId,
                runtimePack,
                PEStreamOptions.PrefetchEntireImage, readerOptions);
        }
    }
}