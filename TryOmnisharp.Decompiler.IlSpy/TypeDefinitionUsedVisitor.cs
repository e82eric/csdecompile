using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharp.Decompiler.IlSpy;

class TypeDefinitionUsedVisitor : TypeVisitor
{
    public readonly ITypeDefinition TypeDefinition;

    public bool Found { get; set; }

    readonly bool topLevelOnly;

    public TypeDefinitionUsedVisitor(ITypeDefinition definition, bool topLevelOnly)
    {
        this.TypeDefinition = definition;
        this.topLevelOnly = topLevelOnly;
    }

    public override IType VisitTypeDefinition(ITypeDefinition type)
    {
        Found |= TypeDefinition.MetadataToken == type.MetadataToken
                 && TypeDefinition.ParentModule.PEFile == type.ParentModule.PEFile;
        return base.VisitTypeDefinition(type);
    }

    public override IType VisitParameterizedType(ParameterizedType type)
    {
        if (topLevelOnly)
            return type.GenericType.AcceptVisitor(this);
        return base.VisitParameterizedType(type);
    }
}
