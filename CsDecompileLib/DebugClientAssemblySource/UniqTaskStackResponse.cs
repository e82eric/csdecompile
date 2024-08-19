using System.Collections.Generic;

namespace CsDecompileLib.DebugClientAssemblySource;

public class UniqTaskStackResponse
{
    public IReadOnlyList<UniqTaskStackItem> Result { get; set; }
    public bool Success { get; set; }
}