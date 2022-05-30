using System.Composition;
using System.Threading.Tasks;
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

    public async Task<Decompiler> Get(string projectAssemblyFilePath)
    {
        var typeSystem = await _typeSystemFactory.GetTypeSystem(projectAssemblyFilePath);
        var result = new Decompiler(typeSystem);
        return result;
    }
    
    public async Task<Decompiler> Get(DecompilerTypeSystem typeSystem)
    {
        var result = new Decompiler(typeSystem);
        return result;                                            
    }
}