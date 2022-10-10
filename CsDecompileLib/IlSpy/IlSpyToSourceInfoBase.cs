using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.IlSpy;

public class IlSpyToSourceInfoBase
{
    protected DecompileInfo MapToSourceInfo(string[] lines, AstNode usage, ITypeDefinition typeToSearch)
    {
        var line = lines[usage.StartLocation.Line - 1].Trim();
        var metadataSource = new DecompileInfo
        {
            SourceText = line,
        };
        FillFromAstNode(metadataSource, usage);
        FillFromContainingType(metadataSource, typeToSearch);
        return metadataSource;
    }
    
    protected void FillFromAstNode(DecompileInfo target, AstNode usage)
    {
        target.Column = usage.StartLocation.Column;
        target.Line = usage.StartLocation.Line;
        target.StartColumn = usage.StartLocation.Column;
        target.EndColumn = usage.EndLocation.Column;
    }

    protected void FillFromContainingType(DecompileInfo target, ITypeDefinition containingType)
    {
        target.AssemblyName = containingType.ParentModule.AssemblyName;
        target.ContainingTypeFullName = containingType.ReflectionName;
        target.ContainingTypeShortName = containingType.Name;
        target.AssemblyFilePath = containingType.Compilation.MainModule.PEFile.FileName;
        target.NamespaceName = containingType.Namespace;
    }

    protected void MapToSourceInfos(
        ITypeDefinition containingTypeDefinition,
        string sourceText,
        IEnumerable<AstNode> foundUses,
        List<DecompileInfo> result)
    {
        var lines = sourceText.Split(new[] { "\r\n" }, StringSplitOptions.None);

        foreach (var foundUse in foundUses)
        {
            var sourceInfo = MapToSourceInfo(lines, foundUse, containingTypeDefinition);
            result.Add(sourceInfo);
        }
    }
}