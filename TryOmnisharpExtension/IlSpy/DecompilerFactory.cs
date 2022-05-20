using System.Composition;
using System.Threading.Tasks;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class DecompilerFactory
{
    private readonly IlSpyTypeSystemFactory _typeSystemFactory;

    [ImportingConstructor]
    public DecompilerFactory(IlSpyTypeSystemFactory typeSystemFactory)
    {
        _typeSystemFactory = typeSystemFactory;
    }

    public async Task<Decompiler> Get(string projectAssemblyFilePath)
    {
        var typeSystem = await _typeSystemFactory.GetTypeSystem(projectAssemblyFilePath);
        var result = new Decompiler(typeSystem);
        return result;
    }
}