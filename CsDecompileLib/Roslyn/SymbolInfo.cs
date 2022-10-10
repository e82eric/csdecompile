using System.Collections.Generic;

namespace CsDecompileLib.Roslyn;

public class SymbolInfo
{
    public Dictionary<string, object> Properties { get; }
    public SymbolInfo()
    {
        Properties = new Dictionary<string, object>();
    }
}