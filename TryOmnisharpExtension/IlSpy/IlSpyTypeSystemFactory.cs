using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension
{
    [Shared]
    [Export(typeof(IDecompilerTypeSystemFactory))]
    public class IlSpyTypeSystemFactory : IDecompilerTypeSystemFactory
    {
        private readonly Dictionary<string, DecompilerTypeSystem> _projectTypeSystemCache = new();
        private readonly AssemblyResolverFactory _resolverFactory;
        private readonly IOmnisharpWorkspace _omnisharpWorkspace;
        private readonly PeFileCache _peFileCache;
        private Task _loadingTask;

        [System.Composition.ImportingConstructor]
        public IlSpyTypeSystemFactory(
            AssemblyResolverFactory resolverFactory,
            IOmnisharpWorkspace omnisharpWorkspace,
            PeFileCache peFileCache)
        {
            _resolverFactory = resolverFactory;
            _omnisharpWorkspace = omnisharpWorkspace;
            _peFileCache = peFileCache;

            var paths = _omnisharpWorkspace.GetProjectAssemblyPaths();
            _loadingTask = LoadProjects(paths);
        }
        
        private async Task LoadProjects(IEnumerable<string> projectAssemblyPaths)
        {
            foreach (var path in projectAssemblyPaths)
            {
                var projectPeFile = await OpenAssembly(path);
                await Add(projectPeFile);
            }
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

        private async Task<DecompilerTypeSystem> Add(PEFile module)
        {
            var resolver = await _resolverFactory.GetAssemblyResolver(module);
            var typeSystem = new DecompilerTypeSystem(module, resolver);
            _projectTypeSystemCache.Add(module.FileName, typeSystem);
            return typeSystem;
        }
        
        public async Task<DecompilerTypeSystem> GetTypeSystem(string projectDllFilePath)
        {
            await _loadingTask;
            if(_projectTypeSystemCache.TryGetValue(projectDllFilePath, out var result))
            {
                return result;
            }
            var peFile = await _peFileCache.OpenAsync(projectDllFilePath);
            result = await Add(peFile);

            return result;
        }
    }
}