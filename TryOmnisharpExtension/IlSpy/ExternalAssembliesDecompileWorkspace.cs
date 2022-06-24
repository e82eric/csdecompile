using System.Collections.Generic;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;
using Microsoft.CodeAnalysis;

namespace TryOmnisharpExtension.IlSpy;

public class ExternalAssembliesDecompileWorkspace : IDecompileWorkspace
{
    public PEFile[] GetAssemblies()
    {
        return new PEFile[] { };
    }

    public int LoadDlls()
    {
        return 0;
    }

    public IReadOnlyList<Compilation> GetProjectCompilations()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<PEFile> GetWithReferenceAssemblies()
    {
        throw new System.NotImplementedException();
    }
}