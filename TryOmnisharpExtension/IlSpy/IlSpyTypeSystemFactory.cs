using System.Collections.Concurrent;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy
{
    public class IlSpyTypeSystemFactory : IDecompilerTypeSystemFactory
    {
        private readonly ConcurrentDictionary<string, DecompilerTypeSystem> _projectTypeSystemCache = new();
        private readonly AssemblyResolverFactory _resolverFactory;
        private readonly PeFileCache _peFileCache;

        public IlSpyTypeSystemFactory(
            AssemblyResolverFactory resolverFactory,
            PeFileCache peFileCache)
        {
            _resolverFactory = resolverFactory;
            _peFileCache = peFileCache;
        }
        
        private DecompilerTypeSystem Add(PEFile module)
        {
            var resolver = _resolverFactory.GetAssemblyResolver(module);
            var typeSystem = new DecompilerTypeSystem(module, resolver);
            _projectTypeSystemCache.TryAdd(module.FileName, typeSystem);
            return typeSystem;
        }
        
        public DecompilerTypeSystem GetTypeSystem(string projectDllFilePath)
        {
            if(_projectTypeSystemCache.TryGetValue(projectDllFilePath, out var result))
            {
                return result;
            }
            
            if(_peFileCache.TryOpen(projectDllFilePath, out var peFile))
            {
                result = Add(peFile);
                return result;
            }
            return null;
        }
    }
}