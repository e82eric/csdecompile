using ICSharpCode.Decompiler.Metadata;

namespace TryOmnisharpExtension.IlSpy;

public interface IDecompileWorkspace
{
    PEFile[] GetAssemblies();
    void LoadDlls();
}