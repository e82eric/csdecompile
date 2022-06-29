using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;
using Microsoft.CodeAnalysis;

namespace TryOmnisharpExtension.IlSpy;

public interface IDecompileWorkspace
{
    PEFile GetAssembly(string filePath);
    PEFile[] GetAssemblies();
    IReadOnlyList<Compilation> GetProjectCompilations();
    void LoadDllsInDirectory(DirectoryInfo directory);
}