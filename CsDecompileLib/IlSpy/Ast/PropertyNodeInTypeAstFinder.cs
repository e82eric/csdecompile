using CsDecompileLib.GotoDefinition;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.IlSpy.Ast;

public class PropertyNodeInTypeAstFinder : IDefinitionInDecompiledSyntaxTreeFinder<IProperty>
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
            // Only the declaration: the same property is also the symbol of every place that reads or writes it
            if (symbol is IProperty entity && node.IsMemberNode())
            {
                if (entity.Setter != null && setterEntityHandle != null)
                {
                    if (entity.Setter.AreSameUsingToken(setterEntityHandle))
                    {
                        return node.FindNameIdentifier(symbol.Name);
                    }
                }

                if (entity.Getter != null && getterEntityHandle != null)
                {
                    if (entity.Getter.AreSameUsingToken(getterEntityHandle))
                    {
                        return node.FindNameIdentifier(symbol.Name);
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
