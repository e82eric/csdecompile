using System.IO;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;

namespace TryOmnisharpExtension.IlSpy
{
    sealed class MyAssemblyResolver : IAssemblyResolver
    {
        private readonly PeFileCache _peFileCache;
        private readonly string _targetFrameworkId;
        
        private readonly UniversalAssemblyResolver _universalAssemblyResolver;

        public MyAssemblyResolver(
            PeFileCache peFileCache,
            string targetFrameworkId,
            UniversalAssemblyResolver universalAssemblyResolver)
        {
            _targetFrameworkId = targetFrameworkId;
            _universalAssemblyResolver = universalAssemblyResolver;
            _peFileCache = peFileCache;
        }

        public PEFile Resolve(IAssemblyReference reference)
        {
            // return _universalAssemblyResolver.Resolve(reference);
            PEFile result = null;
            if (_peFileCache.TryGetByNameAndFrameworkId(reference.FullName, _targetFrameworkId, out result))
            {
                return result;
            }
            
            string file = _universalAssemblyResolver.FindAssemblyFile(reference);
            if (file != null)
            {
                if (_peFileCache.TryOpen(file, out result))
                {
                    return result;
                }
                return null;
            }
            if (_peFileCache.TryGetFirstMatchByName(reference.FullName, out result))
            {
                return result;
            }
            
            return null;
        }

        public PEFile ResolveModule(PEFile mainModule, string moduleName)
        {
            // return _universalAssemblyResolver.ResolveModule(mainModule, moduleName);
            PEFile result = null;
            if (_peFileCache.TryGetByNameAndFrameworkId(moduleName, _targetFrameworkId, out result))
            {
                return result;
            }
            
            var moduleFromResolver = _universalAssemblyResolver.ResolveModule(mainModule, moduleName);
            if (moduleFromResolver != null)
            {
                return moduleFromResolver;
            }
            
            string file = Path.Combine(Path.GetDirectoryName(mainModule.FileName), moduleName);
            if (File.Exists(file))
            {
                if (_peFileCache.TryOpen(file, out result))
                {
                    return result;
                }

                return null;
            }
            if (_peFileCache.TryGetFirstMatchByName(moduleName, out result))
            {
                return result;
            }
            
            return null;
        }
        
        public Task<PEFile> ResolveAsync(IAssemblyReference reference)
        {
            var result = Task.Run(() => Resolve(reference));
            return result;
        }

        public Task<PEFile> ResolveModuleAsync(PEFile mainModule, string moduleName)
        {
            var result = Task.Run(() => ResolveModule(mainModule, moduleName));
            return result;
        }
    }
}