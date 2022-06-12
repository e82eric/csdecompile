using System.Collections.Generic;
using System.Composition;
using System.Linq;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GetMembers;

[Export]
public class TypeMembersFinder
{
    public IEnumerable<AstNode> Find(
        SyntaxTree syntaxTree,
        ITypeDefinition typeToSearchEntityHandle)
    {
        var usageNodes = new List<AstNode>();
        var typeNode = syntaxTree.FindChildType(typeToSearchEntityHandle.MetadataToken);
        if (typeNode != null)
        {
            FindUsages(typeNode, usageNodes);
        }

        return usageNodes;
    }
    
    private void FindUsages(AstNode node, IList<AstNode> found)
    {
        if (node.NodeType == NodeType.Member)
        {
            //This recurses through the entire class and finds any nodes that reference the members token
            var symbol = node.GetSymbol();
            if (symbol is IMember)
            {
                var modifierNode = node.Children.FirstOrDefault(c => !(c is AttributeSection));
                if (modifierNode != null)
                {
                    found.Add(modifierNode);
                }
                else
                {
                    found.Add(node);
                }
            }
        }

        foreach (var child in node.Children)
        {
            FindUsages(child, found);
        }
    }
}