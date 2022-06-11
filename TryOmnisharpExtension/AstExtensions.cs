using System.Collections.Generic;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension;

public static class AstExtensions
{
    public static AstNode FindChildType(this AstNode node, EntityHandle entityHandle)
    {
        var symbol = node.GetSymbol();
        if (symbol is IEntity entity)
        {
            if (entity.MetadataToken == entityHandle)
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
    public static AstNode FindChildMethod(this AstNode node, EntityHandle entityHandle)
    {
        if (node.NodeType == NodeType.Member)
        {
            var symbol = node.GetSymbol();
            if (symbol is IEntity entity)
            {
                if (entity.MetadataToken == entityHandle)
                {
                    if (node is MethodDeclaration || node is ConstructorDeclaration)
                    {
                        return node;
                    }
                    
                    return null;
                }
            }
        }

        if (node.HasChildren)
        {
            foreach (var child in node.Children)
            {
                var method = FindChildMethod(child, entityHandle);
                if (method != null)
                {
                    return method;
                }
            }
        }

        return null;
    }
    
    public static void FindChildUsages(this AstNode node, EntityHandle entityHandle, IList<AstNode> result)
    {
        var symbol = node.GetSymbol();
        if (symbol is IEntity entity)
        {
            if (entity.MetadataToken == entityHandle)
            {
                result.Add(node);
            }
        }

        if (node.HasChildren)
        {
            foreach (var child in node.Children)
            {
                FindChildUsages(child, entityHandle, result);
            }
        }
    }
}