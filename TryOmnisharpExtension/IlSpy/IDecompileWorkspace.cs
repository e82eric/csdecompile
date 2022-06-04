using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;

namespace TryOmnisharpExtension.IlSpy;

public interface IDecompileWorkspace
{
    Task<PEFile[]> GetAssemblies();
    Task LoadDlls();
}