using System;

namespace CsDecompileLib.IlSpy;

public class DecompilerFactory
{
    private readonly IDecompilerTypeSystemFactory _typeSystemFactory;

    public DecompilerFactory(
        IDecompilerTypeSystemFactory typeSystemFactory)
    {
        _typeSystemFactory = typeSystemFactory;
    }

    public Decompiler Get(string projectAssemblyFilePath)
    {
        var typeSystem = _typeSystemFactory.GetTypeSystem(projectAssemblyFilePath);
        if (typeSystem == null)
        {
            throw new Exception($"Could not load type system for {projectAssemblyFilePath}");
        }
        var result = new Decompiler(typeSystem);
        return result;
    }
}