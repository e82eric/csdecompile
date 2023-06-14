using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler.Metadata;

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

    public static void FillFromAssemblyReference(this SymbolInfo symbolInfo, IAssemblyReference assemblyReference)
    {
        symbolInfo.ParentAssemblyFullName = assemblyReference.FullName;
        symbolInfo.ParentAssemblyName = assemblyReference.Name;
        if (assemblyReference.Version != null)
        {
            symbolInfo.ParentAssemblyMajorVersion = assemblyReference.Version.Major;
            symbolInfo.ParentAssemblyMinorVersion = assemblyReference.Version.Minor;
            symbolInfo.ParentAssemblyBuildVersion = assemblyReference.Version.Build;
        }
    }
}