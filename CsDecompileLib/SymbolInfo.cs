using System.Collections.Generic;

namespace CsDecompileLib.Roslyn;

public class SymbolInfo
{
    public int? ParentAssemblyMajorVersion { get; set; }
    public int? ParentAssemblyMinorVersion { get; set; }
    public int? ParentAssemblyBuildVersion { get; set; }
    public string ParentAssemblyName { get; set; }
    public string ParentAssemblyFullName { get; set; }
    public string TargetFramework { get; set; }
    public string FilePath { get; set; }
    public string DisplayName { get; set; }
    public string Kind { get; set; }
    public Dictionary<string, string> HeaderProperties { get; }
    public Dictionary<string, object> Properties { get; }
    public SymbolInfo()
    {
        Properties = new Dictionary<string, object>();
        HeaderProperties = new Dictionary<string, string>();
    }
}