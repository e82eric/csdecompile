using System.Collections.Generic;
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

    public IEnumerable<PEFile> GetWithReferenceAssemblies()
    {
        throw new System.NotImplementedException();
    }
}