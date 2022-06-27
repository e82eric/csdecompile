using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GetMembers;

public class IlSpyTypeMembersFinder : IlSpyToSourceInfoBase
{
    private readonly TypeMembersFinder _typeMembersFinder;
    private readonly DecompilerFactory _decompilerFactory;

    public IlSpyTypeMembersFinder(
        TypeMembersFinder typeMembersFinder,
        DecompilerFactory decompilerFactory)
    {
        _typeMembersFinder = typeMembersFinder;
        _decompilerFactory = decompilerFactory;
    }
        
    public IEnumerable<DecompileInfo> Run(ITypeDefinition containingTypeDefinition)
    {
        var result = new List<DecompileInfo>();

        var decompiledTypeDefintion = DecompileTypeDefinition(containingTypeDefinition);

        var foundUses = _typeMembersFinder.Find(
            decompiledTypeDefintion.syntaxTree,
            containingTypeDefinition);

        MapToSourceInfos(containingTypeDefinition, decompiledTypeDefintion.sourceText, foundUses, result);

        return result;
    }
    
    protected (SyntaxTree syntaxTree, string sourceText) DecompileTypeDefinition(ITypeDefinition typeDefinition)
    {
        var fileName = typeDefinition.ParentModule.PEFile.FileName;
        var cachingDecompiler = _decompilerFactory.Get(fileName);

        var (syntaxTree, sourceText) = cachingDecompiler.Run(typeDefinition);
        return (syntaxTree, sourceText);
    }
}