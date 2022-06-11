using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class DecompilerFactory
{
    private readonly IDecompilerTypeSystemFactory _typeSystemFactory;

    [ImportingConstructor]
    public DecompilerFactory(
        IDecompilerTypeSystemFactory typeSystemFactory)
    {
        _typeSystemFactory = typeSystemFactory;
    }

    public Decompiler Get(string projectAssemblyFilePath)
    {
        var typeSystem = _typeSystemFactory.GetTypeSystem(projectAssemblyFilePath);
        var result = new Decompiler(typeSystem);
        return result;
    }
    
    public Decompiler Get(DecompilerTypeSystem typeSystem)
    {
        var result = new Decompiler(typeSystem);
        return result;                                            
    }
}