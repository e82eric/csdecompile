﻿using System.Collections.Concurrent;
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
        private readonly ICsDecompileWorkspace _workspace;
        private readonly PeFileCache _peFileCache;
        private readonly ConcurrentDictionary<string, Compilation> _compilations = new ();

        public DecompileWorkspace(ICsDecompileWorkspace workspace, PeFileCache peFileCache)
        {
            _workspace = workspace;
            _peFileCache = peFileCache;
        }
        
        public void AddPeFile(PEFile peFile)
        {
            _peFileCache.Add(peFile);
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

            var result = _peFileCache.GetAssemblyCount();
            return result;
        }

        public void LoadDllsInDirectory(DirectoryInfo directory)
        {
            var binDirDlls = directory.GetFiles("*.*", SearchOption.AllDirectories)
                .Where(s => s.Extension.Equals(".dll") || s.Extension.Equals(".exe"));;

            foreach (var dllFilePath in binDirDlls)
            {
                _peFileCache.TryOpen(dllFilePath.FullName, out _);
            }
        }

        public async Task RunProjectCompilations()
        {
            foreach (var project in _workspace.CurrentSolution.Projects)
            {
                if (_compilations.TryGetValue(project.AssemblyName, out var compilation))
                {
                }
                else
                {
                    compilation = await project.GetCompilationAsync();
                    _compilations.TryAdd(project.AssemblyName, compilation);
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