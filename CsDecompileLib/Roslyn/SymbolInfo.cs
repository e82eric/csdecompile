using System.Collections.Generic;
using System.Reflection.Emit;

namespace CsDecompileLib.Roslyn;

public class DisplayPart
{
    public string Kind { get; set; }
    public string Name { get; set; }
}
public class SymbolInfo
{
    public Dictionary<string, object> Properties { get; }
    public SymbolInfo()
    {
        Properties = new Dictionary<string, object>();
    }
}