using ICSharpCode.Decompiler.TypeSystem;
using CsDecompileLib.IlSpy;

namespace CsDecompileLib.FindUsages;

class TypeDefinitionUsedVisitor : TypeVisitor
{
    public readonly ITypeDefinition TypeDefinition;

    public bool Found { get; set; }

    readonly bool _topLevelOnly;

    public TypeDefinitionUsedVisitor(ITypeDefinition definition, bool topLevelOnly)
    {
        TypeDefinition = definition;
        this._topLevelOnly = topLevelOnly;
    }

    public override IType VisitTypeDefinition(ITypeDefinition type)
    {
        Found |= TypeDefinition.AreSameUsingToken(type);
        return base.VisitTypeDefinition(type);
    }

    public override IType VisitParameterizedType(ParameterizedType type)
    {
        if (_topLevelOnly)
            return type.GenericType.AcceptVisitor(this);
        return base.VisitParameterizedType(type);
    }
}