namespace CsDecompileLib.DebugClientAssemblySource;

public class UniqTaskStackFrame
{
    public uint Ordinal { get; set; }
    public ulong InstructionPointer { get; set; }
    public string StateMachineTypeName { get; set; }
    public ulong MethodTable { get; set; }
    public int State { get; set; }
    public int Depth { get; set; }
}