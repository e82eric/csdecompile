using System.Collections.Generic;

namespace CsDecompileLib.DebugClientAssemblySource;

public class UniqCallStackItem
{
    public IReadOnlyList<UniqCallStackFrame> Frames { get; set; }
    public IReadOnlyList<UniqCallStackThread> Threads { get; set; }
}