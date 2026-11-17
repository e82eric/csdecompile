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

    // The identifier that names a declaration, so the location is the name and not an attribute or
    // modifier in front of it. A constructor is named after its type, and an indexer or operator
    // has no name at all: those use the first thing after the attributes.
    public static AstNode FindNameIdentifier(this AstNode declaration, string name)
    {
        foreach (var child in declaration.Children)
        {
            if (child is Identifier identifier && identifier.Name == name)
            {
                return identifier;
            }

            // event EventHandler Changed; keeps the name inside a variable initializer
            if (child is VariableInitializer)
            {
                foreach (var grandChild in child.Children)
                {
                    if (grandChild is Identifier variableName && variableName.Name == name)
                    {
                        return variableName;
                    }
                }
            }
        }

        foreach (var child in declaration.Children)
        {
            if (child is Identifier)
            {
                return child;
            }
        }

        foreach (var child in declaration.Children)
        {
            if (child is not AttributeSection)
            {
                return child;
            }
        }

        return declaration;
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

    // The declaration a variable at this node belongs to: the enclosing member, or the type when
    // the node is in a primary constructor's parameter list.
    public static AstNode FindVariableScopeNode(this AstNode node)
    {
        var member = node.FindParentMemberNode();
        if (member != null)
        {
            return member;
        }

        for (var ancestor = node; ancestor != null; ancestor = ancestor.Parent)
        {
            if (ancestor is TypeDeclaration)
            {
                return ancestor;
            }
        }

        return null;
    }

    public static AstNode FindParentMemberNode(this AstNode node)
    {
        if (node == null)
        {
            return null;
        }

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