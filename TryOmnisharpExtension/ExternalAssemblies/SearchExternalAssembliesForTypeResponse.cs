using System.Collections.Generic;

namespace TryOmnisharpExtension.ExternalAssemblies;

public class SearchExternalAssembliesForTypeResponse
{
    public IEnumerable<DecompileInfo> FoundTypes { get; set; }
}