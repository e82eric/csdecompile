using System;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib;

public static class DecompileInfoExtensions
{
    public static void FillFromContainingType(this DecompileInfo target, ITypeDefinition containingType)
    {
        //These are exceptional cases and we want the request ot fail.  We look these up from existing assemblies so it should never happen
        if (containingType.ParentModule == null)
        {
            throw new Exception($"Parent module of '{containingType.Name}' in '{containingType.ReflectionName}' was null");
        }
        
        if (containingType.Compilation.MainModule.MetadataFile == null)
        {
            throw new Exception($"Metadata file for main module of '{containingType.Name}' in '{containingType.ReflectionName}' was null");
        }
        
        if (containingType.ParentModule.MetadataFile == null)
        {
            throw new Exception($"Metadata file of parent module of '{containingType.Name}' in '{containingType.ReflectionName}' was null");
        }

        target.AssemblyName = containingType.ParentModule.AssemblyName;
        target.ContainingTypeFullName = containingType.ReflectionName;
        target.ContainingTypeShortName = containingType.Name;
        target.AssemblyFilePath = containingType.Compilation.MainModule.MetadataFile.FileName;
        target.NamespaceName = containingType.Namespace;
        target.ParentAssemblyFilePath = containingType.ParentModule.MetadataFile.FileName;
    }
    
    public static void FillFromAstNode(this DecompileInfo target, AstNode usage)
    {
        target.Column = usage.StartLocation.Column;
        target.Line = usage.StartLocation.Line;
        target.StartColumn = usage.StartLocation.Column;
        target.EndColumn = usage.EndLocation.Column;
    }
}