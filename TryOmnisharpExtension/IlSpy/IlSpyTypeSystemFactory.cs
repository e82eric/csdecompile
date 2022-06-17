using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy
{
    [Shared]
    [Export(typeof(IDecompilerTypeSystemFactory))]
    public class IlSpyTypeSystemFactory : IDecompilerTypeSystemFactory
    {
        private readonly ConcurrentDictionary<string, DecompilerTypeSystem> _projectTypeSystemCache = new();
        private readonly AssemblyResolverFactory _resolverFactory;
        private readonly PeFileCache _peFileCache;

        [ImportingConstructor]
        public IlSpyTypeSystemFactory(
            AssemblyResolverFactory resolverFactory,
            IOmnisharpWorkspace omnisharpWorkspace,
            PeFileCache peFileCache)
        {
            _resolverFactory = resolverFactory;
            _peFileCache = peFileCache;

            var paths = omnisharpWorkspace.GetProjectAssemblyPaths();
            //TODO: This should be somewhere else
            LoadProjects(paths);
        }
        
        private void LoadProjects(IEnumerable<string> projectAssemblyPaths)
        {
            foreach (var path in projectAssemblyPaths)
            {
                var projectPeFile = OpenAssembly(path);
                if (projectPeFile != null)
                {
                    Add(projectPeFile);
                }
            }
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
            var peFile = _peFileCache.Open(projectDllFilePath);
            result = Add(peFile);

            return result;
        }
    }
}