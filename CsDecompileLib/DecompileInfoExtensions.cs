using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib;

public static class DecompileInfoExtensions
{
    public static void FillFromContainingType(this DecompileInfo target, ITypeDefinition containingType)
    {
        target.AssemblyName = containingType.ParentModule.AssemblyName;
        target.ContainingTypeFullName = containingType.ReflectionName;
        target.ContainingTypeShortName = containingType.Name;
        target.AssemblyFilePath = containingType.Compilation.MainModule.PEFile.FileName;
        target.NamespaceName = containingType.Namespace;
        target.ParentAssemblyFilePath = containingType.ParentModule.PEFile.FileName;
    }
    
    public static void FillFromAstNode(this DecompileInfo target, AstNode usage)
    {
        target.Column = usage.StartLocation.Column;
        target.Line = usage.StartLocation.Line;
        target.StartColumn = usage.StartLocation.Column;
        target.EndColumn = usage.EndLocation.Column;
    }
}