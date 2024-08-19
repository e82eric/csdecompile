using System.Collections.Generic;

namespace CsDecompileLib.DebugClientAssemblySource;

public class UniqTaskStackItem
{
    public UniqTaskStackItem(IReadOnlyList<UniqTaskStackFrame> frames, ulong firstTask)
    {
        Frames = frames;
        Tasks = new List<ulong>();
        Tasks.Add(firstTask);
    }
    public IReadOnlyList<UniqTaskStackFrame> Frames { get; private set; }
    public IList<ulong> Tasks { get; }
}