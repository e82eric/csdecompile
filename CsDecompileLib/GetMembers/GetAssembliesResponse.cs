using System.Collections.Generic;

namespace CsDecompileLib.GetMembers;

public class GetAssembliesResponse
{
    public IEnumerable<Assembly> Assemblies { get; set; }
}