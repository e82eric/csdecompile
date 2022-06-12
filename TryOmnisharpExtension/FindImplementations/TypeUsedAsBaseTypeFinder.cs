using System.Collections.Generic;
using System.Composition;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.FindUsages;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindImplementations;

[Export]
public class TypeUsedAsBaseTypeFinder : IEntityUsedInTypeFinder<ITypeDefinition>
{
    public IEnumerable<AstNode> Find(
        SyntaxTree syntaxTree,
        ITypeDefinition typeToSearchEntityHandle,
        ITypeDefinition usageToFind)
    {
        var usageNodes = new List<AstNode>();
        var typeNode = syntaxTree.FindChildType(typeToSearchEntityHandle.MetadataToken);
        if (typeNode != null)
        {
            FindUsages(typeNode, usageToFind.MetadataToken, usageNodes);
        }

        return usageNodes;
    }
    
    private void FindUsages(AstNode node, EntityHandle entityHandleToSearchFor, IList<AstNode> found)
    {
        if (node.Role != Roles.TypeMemberRole)
        {
            if (node.Role == Roles.BaseType)
            {
                //This should basically recurse through the entire class and find any nodes that reference the types token
                var symbol = node.GetSymbol();
                if (symbol is IEntity entity)
                {
                    if (entity.MetadataToken == entityHandleToSearchFor)
                    {
                        found.Add(node);
                    }
                }
            }

            foreach (var child in node.Children)
            {
                FindUsages(child, entityHandleToSearchFor, found);
            }
        }
    }
}