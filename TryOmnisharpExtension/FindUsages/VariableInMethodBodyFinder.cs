using System.Collections.Generic;
using System.Composition;
using ICSharpCode.Decompiler.CSharp.Syntax;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages;

[Export]
public class VariableInMethodBodyFinder
{
    public IEnumerable<UsageAsTextLocation> Find(Identifier variableNode)
    {
        var result = new List<UsageAsTextLocation>();

        var methodBodyNode = FindMethodBody(variableNode);
        if (methodBodyNode != null)
        {
            var foundUsages = new List<Identifier>();
            FindUsagesOfIdentifier(methodBodyNode, variableNode, foundUsages);

            foreach (var identifier in foundUsages)
            {
                var statement = FindIdentifierParentStatement(identifier);
                var usage = new UsageAsTextLocation
                {
                    StartLocation = identifier.StartLocation,
                    EndLocation = identifier.EndLocation,
                    Statement = statement.ToString()
                };
                result.Add(usage);
            }
        }

        return result;
    }

    private AstNode FindMethodBody(AstNode node)
    {
        //TODO: Figure out if you can go directly to body
        if (node is MethodDeclaration methodDeclaration)
        {
            return methodDeclaration.Body;
        }

        if (node.Parent != null)
        {
            var result = FindMethodBody(node.Parent);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
    
    private void FindUsagesOfIdentifier(AstNode currentNode, Identifier variableNode, IList<Identifier> found)
    {
        if (currentNode is Identifier identifier)
        {
            if (identifier.Name == variableNode.Name)
            {
                found.Add(identifier);
            }
        }
        
        foreach (var child in currentNode.Children)
        {
            FindUsagesOfIdentifier(child, variableNode, found);
        }
    }

    private Statement FindIdentifierParentStatement(AstNode currentNode)
    {
        if (currentNode is Statement statement)
        {
            return statement;
        }

        if (currentNode.Parent != null)
        {
            var result = FindIdentifierParentStatement(currentNode.Parent);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}