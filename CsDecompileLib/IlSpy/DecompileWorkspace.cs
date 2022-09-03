using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;
using Microsoft.CodeAnalysis;

namespace CsDecompileLib.IlSpy
{
    public class DecompileWorkspace : IDecompileWorkspace
    {
        private readonly IOmniSharpWorkspace _workspace;
        private readonly PeFileCache _peFileCache;
        private readonly ConcurrentDictionary<string, Compilation> _compilations = new ();

        public DecompileWorkspace(IOmniSharpWorkspace workspace, PeFileCache peFileCache)
        {
            _workspace = workspace;
            _peFileCache = peFileCache;
        }

        public int LoadDlls()
        {
            var projectAssemblyPaths = _workspace.GetProjectAssemblyPaths();

            Parallel.ForEach(projectAssemblyPaths, path =>
            {
                var projectDllFile = new FileInfo(path);
                var projectBinDir = projectDllFile.Directory;
                LoadDllsInDirectory(projectBinDir);
            });
            // foreach (var path in projectAssemblyPaths)
            // {
            //     var projectDllFile = new FileInfo(path);
            //     var projectBinDir = projectDllFile.Directory;
            //     var binDirDlls = projectBinDir.GetFiles("*.dll", SearchOption.AllDirectories);
            //
            //     foreach (var dllFilePath in binDirDlls)
            //     {
            //         _peFileCache.TryOpen(dllFilePath.FullName, out _);
            //     }
            // }

            var result = _peFileCache.GetAssemblyCount();
            return result;
        }

        public void LoadDllsInDirectory(DirectoryInfo directory)
        {
            var binDirDlls = directory.GetFiles("*.dll", SearchOption.AllDirectories);

            foreach (var dllFilePath in binDirDlls)
            {
                _peFileCache.TryOpen(dllFilePath.FullName, out _);
            }
        }

        public async Task RunProjectCompilations()
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
        }

        public IReadOnlyList<Compilation> GetProjectCompilations()
        {
            var result = _compilations.Values.ToArray();
            return result;
        }

        public PEFile GetAssembly(string filePath)
        {
            _peFileCache.TryOpen(filePath, out var result);
            return result;
        }

        public PEFile[] GetAssemblies()
        {
            var result = _peFileCache.GetAssemblies();
            return result;
        }
    }
}