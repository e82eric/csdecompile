using System.Collections.Generic;
using System.Reflection.Emit;

namespace TryOmnisharpExtension.Roslyn;

public class DisplayPart
{
    public string Kind { get; set; }
    public string Name { get; set; }
}
public class SymbolInfo
{
    public string Result { get; set; }
    public string FullName { get; set; }
    public string Name { get; set; }
    public string Namespace { get; set; }
    public string Kind { get; set; }
    public string ReturnType { get; set; }
    public Dictionary<string, object> Properties { get; }

    public SymbolInfo()
    {
        Properties = new Dictionary<string, object>();
    }
}