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
        metadataSource.FillFromAstNode(usage);
        metadataSource.FillFromContainingType(typeToSearch);
        return metadataSource;
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