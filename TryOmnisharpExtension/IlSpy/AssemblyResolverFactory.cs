using System.Composition;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;

namespace TryOmnisharpExtension.IlSpy
{
    [Export]
    public class AssemblyResolverFactory
    {
        private readonly PeFileCache _peFileCache;

        [ImportingConstructor]
        public AssemblyResolverFactory(PeFileCache peFileCache)
        {
            _peFileCache = peFileCache;
        }
        
        public async Task<IAssemblyResolver> GetAssemblyResolver(PEFile peFile)
        {
            var targetFrameworkId = peFile.DetectTargetFrameworkId();
            var universalResolver = await GetUniversalResolver(targetFrameworkId, peFile);
            return new MyAssemblyResolver(_peFileCache, targetFrameworkId, universalResolver);
        }
        
        private async Task<UniversalAssemblyResolver> GetUniversalResolver(string targetFrameworkId, PEFile peFile)
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