using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;

namespace CsDecompileLib.FindUsages;

public class VariableInMethodBodyFinder
{
    public IEnumerable<AstNode> Find(AstNode methodBodyNode, ILVariable variable)
    {
        var foundUsages = new List<AstNode>();
        if (methodBodyNode != null)
        {
            FindUsagesOfIdentifier(methodBodyNode, variable, foundUsages);
        }

        return foundUsages;
    }
    
    private void FindUsagesOfIdentifier(AstNode currentNode, ILVariable variable, ICollection<AstNode> found)
    {
        if (currentNode is Identifier identifier)
        {
            if (identifier.Name == variable.Name)
            {
                found.Add(identifier);
            }
        }
        
        foreach (var child in currentNode.Children)
        {
            FindUsagesOfIdentifier(child, variable, found);
        }
    }
}