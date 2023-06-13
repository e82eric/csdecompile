using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GetSymbolInfo;

public class IlSpySymbolInfoCommandBase
{
    protected void AddIlSpyEntityCommonHeaderProperties(SymbolInfo result, IEntity symbol)
    {
        result.ParentAssemblyFullName = symbol.ParentModule.FullAssemblyName;
        result.TargetFramework = symbol.ParentModule.PEFile?.DetectTargetFrameworkId();
        result.FilePath = symbol.ParentModule.PEFile?.FileName;
        // AddIlSpyEntityCommonHeaderProperties(result, symbol);
        // AddIlSpyEntityCommonHeaderProperties(result, (ISymbol)symbol);
        AddNameAndKind(result, symbol.Name, symbol.SymbolKind.ToString());
        AddNamespaceAndAssemblyPath(
            result,
            symbol);
    }

    protected void AddIlSpyVariableCommonHeaderProperties(SymbolInfo result, ILVariable symbol)
    {
        AddNameAndKind(result, symbol.Name, symbol.Kind.ToString());
        AddNamespaceAndAssemblyPath(result, symbol.Function.DelegateType.GetDefinition());
    }

    protected void AddIlSpyEntityCommonHeaderProperties(SymbolInfo result, ISymbol symbol)
    {
        AddNameAndKind(result, symbol.Name, symbol.SymbolKind.ToString());
    }

    protected void AddNameAndKind(SymbolInfo result, string name, string kind)
    {
        result.DisplayName = name;
        result.Kind = kind;
        result.HeaderProperties.Add("Name", name);
        result.HeaderProperties.Add("Kind", kind);
    }

    protected void AddNamespaceAndAssemblyPath(SymbolInfo result, IEntity symbol)
    {
        result.HeaderProperties.Add("AssemblyPath", symbol.ParentModule.PEFile.FileName);
        result.HeaderProperties.Add("Namespace", symbol.Namespace);
    }
}