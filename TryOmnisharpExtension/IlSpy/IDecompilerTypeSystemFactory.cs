using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension;

public interface IDecompilerTypeSystemFactory
{
    DecompilerTypeSystem GetTypeSystem(string projectDllFilePath);
}