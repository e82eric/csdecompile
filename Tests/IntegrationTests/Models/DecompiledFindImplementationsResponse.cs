using System.Collections.Generic;
using CsDecompileLib;

namespace IntegrationTests;

public class DecompiledFindImplementationsResponse
{
    public DecompiledFindImplementationsResponse()
    {
        Locations = new List<DecompileInfo>();
    }
    public IList<DecompileInfo> Locations { get; }
}