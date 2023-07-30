using System.Collections.Generic;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.Metadata;

namespace CsDecompileLib.GetMembers;

public class GetAssembliesCommand
{
    private readonly IDecompileWorkspace _decompileWorkspace;

    public GetAssembliesCommand(IDecompileWorkspace decompileWorkspace)
    {
        _decompileWorkspace = decompileWorkspace;
    }

    public GetAssembliesResponse Execute()
    {
        var assemblies = _decompileWorkspace.GetAssemblies();
        var fullNames = new List<Assembly>();
        foreach (var assembly in assemblies)
        {
            var frameworkId = assembly.DetectTargetFrameworkId2();
            var assemblyPoco = new Assembly()
            {
                FilePath = assembly.FileName,
                FullName = assembly.Name,
                TargetFrameworkId = frameworkId
            };
            fullNames.Add(assemblyPoco);
        }

        var result = new GetAssembliesResponse { Assemblies = fullNames };
        return result;
    }
}