using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.IlSpy;

public static class AstExtensions
{
    // ILSpy 11 removed AstNode.NodeType; these mirror the old NodeType.TypeDeclaration / NodeType.Member values.
    public static bool IsTypeDeclarationNode(this AstNode node)
    {
        return node is TypeDeclaration or DelegateDeclaration;
    }

    public static bool IsMemberNode(this AstNode node)
    {
        return node is EntityDeclaration and not (TypeDeclaration or DelegateDeclaration or Accessor);
    }

    public static AstNode FindChildType(this AstNode node, IEntity entityHandle)
    {
        var symbol = node.GetSymbol();
        if (symbol is IEntity entity)
        {
            if (entity.AreSameUsingToken(entityHandle))
            {
                if (node.IsTypeDeclarationNode())
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
        if (node.IsMemberNode())
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