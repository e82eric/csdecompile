using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;
using Microsoft.CodeAnalysis;

namespace TryOmnisharpExtension.IlSpy
{
    [Shared]
    [Export(typeof(IDecompileWorkspace))]
    public class DecompileWorkspace : IDecompileWorkspace
    {
        private readonly IOmnisharpWorkspace _workspace;
        private readonly PeFileCache _peFileCache;
        private readonly ConcurrentDictionary<string, Compilation> _compilations = new ();

        [ImportingConstructor]
        public DecompileWorkspace(IOmnisharpWorkspace workspace, PeFileCache peFileCache)
        {
            _workspace = workspace;
            _peFileCache = peFileCache;
        }

        public void LoadDlls()
        {
            var projectAssemblyPaths = _workspace.GetProjectAssemblyPaths();
            foreach (var path in projectAssemblyPaths)
            {
                var projectDllFile = new FileInfo(path);
                var projectBinDir = projectDllFile.Directory;
                var binDirDlls = projectBinDir.GetFiles("*.dll", SearchOption.AllDirectories);

                foreach (var dllFilePath in binDirDlls)
                {
                    _peFileCache.TryOpen(dllFilePath.FullName, out _);
                }
            }
        }

        public async Task<IReadOnlyList<Compilation>> GetProjectCompilations()
        {
            var result = new List<Compilation>();
            foreach (var project in _workspace.CurrentSolution.Projects)
            {
                if (_compilations.TryGetValue(project.AssemblyName, out var compilation))
                {
                   result.Add(compilation);
                }
                else
                {
                    compilation = await project.GetCompilationAsync();
                    _compilations.TryAdd(project.AssemblyName, compilation);
                    result.Add(compilation);
                }
            }
            return result;
        }

		public PEFile[] GetAssemblies()
        {
            LoadDlls();
            var result = _peFileCache.GetAssemblies();
            return result;
        }
    }
}