using System.Collections.Generic;
using CsDecompileLib;

namespace IntegrationTests;

public class DecompiledFindImplementationsResponse
{
    public DecompiledFindImplementationsResponse()
    {
        Implementations = new List<DecompileInfo>();
    }
    public IList<DecompileInfo> Implementations { get; }
}