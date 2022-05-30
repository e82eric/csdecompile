using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension;

public interface IDecompilerTypeSystemFactory
{
    Task<DecompilerTypeSystem> GetTypeSystem(string projectDllFilePath);
}