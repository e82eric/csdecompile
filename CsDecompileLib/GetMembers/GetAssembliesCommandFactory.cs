using CsDecompileLib.IlSpy;

namespace CsDecompileLib.GetMembers;

public class GetAssembliesCommandFactory
{
    private readonly IDecompileWorkspace _decompileWorkspace;

    public GetAssembliesCommandFactory(IDecompileWorkspace decompileWorkspace)
    {
        _decompileWorkspace = decompileWorkspace;
    }

    public GetAssembliesCommand Get()
    {
        return new GetAssembliesCommand(_decompileWorkspace);
    }
}