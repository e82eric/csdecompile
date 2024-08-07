using Microsoft.Diagnostics.Runtime;

namespace CsDecompileLib.DebugClientAssemblySource;

public class DataTargetProvider
{
    private DataTarget _dataTarget;
    public void SetFromProcess(int processId, bool suspend)
    {
        _dataTarget = DataTarget.AttachToProcess(processId, suspend);
    }

    public void SetFromMemoryDump(string filePath)
    {
        _dataTarget = DataTarget.LoadDump(filePath);
    }

    public DataTarget Get()
    {
        return _dataTarget;
    }
}