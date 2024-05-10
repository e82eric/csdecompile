using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.Decompiler.Metadata;
using Microsoft.CodeAnalysis;

namespace CsDecompileLib.IlSpy;

public class NoSolutionDecompileWorkspace : IDecompileWorkspace
{
    private readonly PeFileCache _peFileCache;
    private readonly IlSpyTypeSystemFactory _typeSystemFactory;

    public NoSolutionDecompileWorkspace(PeFileCache peFileCache, IlSpyTypeSystemFactory typeSystemFactory)
    {
        _peFileCache = peFileCache;
        _typeSystemFactory = typeSystemFactory;
    }

    public int LoadDlls()
    {
        var result = _peFileCache.GetAssemblyCount();
        return result;
    }

    public void AddPeFile(PEFile peFile)
    {
        _peFileCache.Add(peFile);
    }

    public void LoadDllsInDirectory(DirectoryInfo directory)
    {
        var binDirDlls = directory.GetFiles("*.*", SearchOption.AllDirectories)
            .Where(s => s.Extension.Equals(".dll") || s.Extension.Equals(".exe"));;

        foreach (var dllFilePath in binDirDlls)
        {
            _peFileCache.TryOpen(dllFilePath.FullName, out _);
        }
        
        _typeSystemFactory.ClearCache();
    }

    public IReadOnlyList<Compilation> GetProjectCompilations()
    {
        var result = Array.Empty<Compilation>();
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