using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.IlSpy;

public static class AstExtensions
{
    public static AstNode FindChildType(this AstNode node, IEntity entityHandle)
    {
        var symbol = node.GetSymbol();
        if (symbol is IEntity entity)
        {
            if (entity.AreSameUsingToken(entityHandle))
            {
                if (node.NodeType == NodeType.TypeDeclaration)
                {
                    return node;
                }
                
                return null;
            }
        }

        if (node.HasChildren)
        {
            foreach (var child in node.Children)
            {
                var method = FindChildType(child, entityHandle);
                if (method != null)
                {
                    return method;
                }
            }
        }

        return null;
    }

    public static AstNode FindParentMemberNode(this AstNode node)
    {
        if (node.NodeType == NodeType.Member)
        {
            return node;
        }

        var result = FindParentMemberNode(node.Parent);
        if (result != null)
        {
            return result;
        }

        return null;
    }
}