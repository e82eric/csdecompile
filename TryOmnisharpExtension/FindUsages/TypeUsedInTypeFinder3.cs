using System;
using System.Collections.Generic;
using System.Composition;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages;

[Export]
public class TypeUsedInTypeFinder3 : IEntityUsedInTypeFinder<ITypeDefinition>
{
    public IEnumerable<UsageAsTextLocation> Find(
        (SyntaxTree SyntaxTree, string SourceText) decompiledTypeDefinition,
        ITypeDefinition typeToSearchEntityHandle,
        ITypeDefinition usageToFind)
    {
        var result = new List<UsageAsTextLocation>();
        var typeNode = decompiledTypeDefinition.SyntaxTree.FindChildType(typeToSearchEntityHandle.MetadataToken);
        if (typeNode != null)
        {
            var usageNodes = new List<AstNode>();
            FindUsages(typeNode, usageToFind.MetadataToken, usageNodes);

            var lines = decompiledTypeDefinition.SourceText.Split(new []{"\r\n"}, StringSplitOptions.None);
            foreach (var node in usageNodes)
            {
                var line = lines[node.StartLocation.Line - 1].Trim();
                var usage = new UsageAsTextLocation
                {
                    StartLocation = node.StartLocation,
                    EndLocation = node.EndLocation,
                    Node = node
                };
                usage.Statement = line;
                result.Add(usage);
            }
        }

        return result;
    }
    
    private void FindUsages(AstNode node, EntityHandle entityHandleToSearchFor, IList<AstNode> found)
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

        foreach (var child in node.Children)
        {
            FindUsages(child, entityHandleToSearchFor, found);
        }
    }
}