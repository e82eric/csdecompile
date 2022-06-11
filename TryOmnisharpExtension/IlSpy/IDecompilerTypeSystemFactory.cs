using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;

public interface IDecompilerTypeSystemFactory
{
    DecompilerTypeSystem GetTypeSystem(string projectDllFilePath);
}