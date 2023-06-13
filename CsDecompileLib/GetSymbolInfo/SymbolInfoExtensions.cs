using CsDecompileLib.Roslyn;

namespace CsDecompileLib.GetSymbolInfo;

public static class SymbolInfoExtensions
{
    public static void FillUnknownTypeProperties(this SymbolInfo symbolInfo)
    {
        symbolInfo.TargetFramework = "Unknown";
        symbolInfo.FilePath = "Unknown";
    }
    public static void AddNameAndKind(this SymbolInfo symbolInfo, string name, string kind)
    {
        symbolInfo.DisplayName = name;
        symbolInfo.Kind = kind;
        symbolInfo.HeaderProperties.Add("Name", name);
        symbolInfo.HeaderProperties.Add("Kind", kind);
    }
}