using System;
using System.Composition;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;

namespace TryOmnisharpExtension.IlSpy
{
    [Export]
    sealed class MyAssemblyResolver : IAssemblyResolver
    {
        private readonly PeFileCache _peFileCache;
        private readonly string _targetFrameworkId;
        
        private readonly UniversalAssemblyResolver _universalAssemblyResolver;

        [ImportingConstructor]
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
            PEFile result = null;
            if (_peFileCache.TryGetByNameAndFrameworkId(reference.FullName, _targetFrameworkId, out result))
            {
                return result;
            }

            string file = _universalAssemblyResolver.FindAssemblyFile(reference);
            if (file != null)
            {
                result = _peFileCache.Open(file);
                return result;
            }
            else
            {
                if (_peFileCache.TryGetFirstMatchByName(reference.FullName, out result))
                {
                    return result;
                }
            }

            return null;
        }

        public PEFile ResolveModule(PEFile mainModule, string moduleName)
        {
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
                result = _peFileCache.Open(file);
                return result;
            }
            else
            {
                // Module does not exist on disk, look for one with a matching name in the assemblylist:
                if (_peFileCache.TryGetFirstMatchByName(moduleName, out result))
                {
                    return result;
                }
            }
            
            return null;
        }
        
        public Task<PEFile> ResolveAsync(IAssemblyReference reference)
        {
            PEFile result = null;
            if (_peFileCache.TryGetByNameAndFrameworkId(reference.FullName, _targetFrameworkId, out result))
            {
                return Task.FromResult(result);
            }

            string file = _universalAssemblyResolver.FindAssemblyFile(reference);
            if (file != null)
            {
                result = _peFileCache.Open(file);
                return Task.FromResult(result);
            }
            else
            {
                if (_peFileCache.TryGetFirstMatchByName(reference.FullName, out result))
                {
                    return Task.FromResult(result);
                }
            }

            return null;
        }

        public async Task<PEFile> ResolveModuleAsync(PEFile mainModule, string moduleName)
        {
            PEFile result = null;
            if (_peFileCache.TryGetByNameAndFrameworkId(moduleName, _targetFrameworkId, out result))
            {
                return result;
            }
            
            var moduleFromResolver = await _universalAssemblyResolver.ResolveModuleAsync(mainModule, moduleName).ConfigureAwait(false);
            if (moduleFromResolver != null)
            {
                return moduleFromResolver;
            }

            string file = Path.Combine(Path.GetDirectoryName(mainModule.FileName), moduleName);
            if (File.Exists(file))
            {
                result = _peFileCache.Open(file);
                return result;
            }
            else
            {
                // Module does not exist on disk, look for one with a matching name in the assemblylist:
                if (_peFileCache.TryGetFirstMatchByName(moduleName, out result))
                {
                    return result;
                }
            }
            
            return null;
        }
    }
}