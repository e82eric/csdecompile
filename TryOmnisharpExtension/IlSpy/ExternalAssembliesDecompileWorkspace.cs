using ICSharpCode.Decompiler.Metadata;

namespace TryOmnisharpExtension.IlSpy;

public class ExternalAssembliesDecompileWorkspace : IDecompileWorkspace
{
    public PEFile[] GetAssemblies()
    {
        return new PEFile[] { };
    }

    public void LoadDlls()
    {
        
    }
}