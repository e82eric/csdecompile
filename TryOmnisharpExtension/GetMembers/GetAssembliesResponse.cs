using System.Collections.Generic;

namespace TryOmnisharpExtension.GetMembers;

public class GetAssembliesResponse
{
    public IEnumerable<Assembly> Assemblies { get; set; }
}