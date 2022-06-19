using System.Composition;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition;

[Export]
public class PropertyInTypeFinder : IDefinitionInDecompiledSyntaxTreeFinder<IProperty>
{
    public AstNode Find(
        IProperty property,
        SyntaxTree rootTypeSyntaxTree)
    {
        var result = Find(
            rootTypeSyntaxTree,
            property.Setter,
            property.Getter);

        return result;
    }

    private AstNode Find(AstNode node, IEntity setterEntityHandle, IEntity getterEntityHandle)
    {
        var symbol = node.GetSymbol();

        if (symbol != null)
        {
            if (symbol is IProperty entity)
            {
                if (entity.Setter != null && setterEntityHandle != null)
                {
                    if (entity.Setter.AreSameUsingToken(setterEntityHandle))
                    {
                        return node;
                    }
                }

                if (entity.Getter != null && getterEntityHandle != null)
                {
                    if (entity.Getter.AreSameUsingToken(getterEntityHandle))
                    {
                        return node;
                    }
                }
            }
        }

        foreach (var child in node.Children)
        {
            var usage = Find(child, setterEntityHandle, getterEntityHandle);
            if (usage != null)
            {
                return usage;
            }
        }

        return null;
    }
}
