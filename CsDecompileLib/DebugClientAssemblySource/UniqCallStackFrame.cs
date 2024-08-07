namespace CsDecompileLib.DebugClientAssemblySource;

public class UniqCallStackFrame
{
    public uint Ordinal { get; set; }
    public ulong StackPointer { get; set; }
    public ulong InstructionPointer { get; set; }
    public string MethodName { get; set; }
    public string TypeName { get; set; }
    public int MetadataToken { get; set; }
}