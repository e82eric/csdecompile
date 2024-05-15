namespace CsDecompileLib.DebugClientAssemblySource;

public class AddProcessAssembliesRequest
{
    public int ProcessId { get; set; }
    public bool Suspend { get; set; }
}