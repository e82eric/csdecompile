using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;
using Microsoft.CodeAnalysis;

namespace TryOmnisharpExtension.IlSpy;

public interface IDecompileWorkspace
{
    PEFile[] GetAssemblies();
    int LoadDlls();
    IReadOnlyList<Compilation> GetProjectCompilations();
    void LoadDllsInDirectory(DirectoryInfo directory);
}