using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.IlSpy;

public interface IDecompilerTypeSystemFactory
{
    DecompilerTypeSystem GetTypeSystem(string projectDllFilePath);
}