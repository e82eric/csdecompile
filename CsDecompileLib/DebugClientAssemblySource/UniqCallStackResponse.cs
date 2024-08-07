using System.Collections.Generic;

namespace CsDecompileLib.DebugClientAssemblySource;

public class UniqCallStackResponse
{
    public IReadOnlyList<UniqCallStackItem> Result { get; set; }
    public bool Success { get; set; }
}