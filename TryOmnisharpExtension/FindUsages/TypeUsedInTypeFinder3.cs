using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages;

public class TypeUsedInTypeFinder3 : IEntityUsedInTypeFinder<ITypeDefinition>
{
    public IEnumerable<AstNode> Find(
        SyntaxTree syntaxTree,
        ITypeDefinition typeToSearchEntityHandle,
        ITypeDefinition usageToFind)
    {
        // var result = new List<UsageAsTextLocation>();
        var usageNodes = new List<AstNode>();
        var typeNode = syntaxTree.FindChildType(typeToSearchEntityHandle);
        if (typeNode != null)
        {
            FindUsages(typeNode, usageToFind, usageNodes);
        }

        return usageNodes;
    }
    
    private void FindUsages(AstNode node, IEntity entityHandleToSearchFor, IList<AstNode> found)
    {
        //This should basically recurse through the entire class and find any nodes that reference the types token
        var symbol = node.GetSymbol();
        if (symbol is IEntity entity)
        {
            if (entity.AreSameUsingToken(entityHandleToSearchFor))
            {
                found.Add(node);
            }
        }

        foreach (var child in node.Children)
        {
            FindUsages(child, entityHandleToSearchFor, found);
        }
    }
}