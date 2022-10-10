using System.Collections.Generic;
using System.IO;
using ICSharpCode.Decompiler.Metadata;
using Microsoft.CodeAnalysis;

namespace CsDecompileLib.IlSpy;

public interface IDecompileWorkspace
{
    PEFile GetAssembly(string filePath);
    PEFile[] GetAssemblies();
    IReadOnlyList<Compilation> GetProjectCompilations();
    void LoadDllsInDirectory(DirectoryInfo directory);
}