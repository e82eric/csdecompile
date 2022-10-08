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

    public NoSolutionDecompileWorkspace(PeFileCache peFileCache)
    {
        _peFileCache = peFileCache;
    }

    public int LoadDlls()
    {
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