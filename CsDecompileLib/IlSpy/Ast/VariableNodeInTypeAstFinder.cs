using System.Linq;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;

namespace CsDecompileLib.IlSpy.Ast;

public class VariableNodeInTypeAstFinder
{
    public AstNode Find(
        ILVariable variableIdentifier,
        AstNode methodNode)
    {
        if (methodNode == null)
        {
            return null;
        }

        // Starting on the type itself means a primary constructor parameter: don't wander into the
        // members, they can declare variables with the same name.
        var usage = methodNode is TypeDeclaration ? null : Find(methodNode, variableIdentifier);
        if (usage != null)
        {
            return usage;
        }

        // class C(int seed) { int Next() => seed + 1; } declares the parameter on the type
        for (var ancestor = methodNode; ancestor != null; ancestor = ancestor.Parent)
        {
            if (ancestor is TypeDeclaration typeDeclaration)
            {
                foreach (var parameter in typeDeclaration.PrimaryConstructorParameters)
                {
                    usage = Find(parameter, variableIdentifier);
                    if (usage != null)
                    {
                        return usage;
                    }
                }
            }
        }

        return null;
    }

    private AstNode Find(AstNode node, ILVariable variable)
    {
        if (node is ParameterDeclaration parameterDeclaration)
        {
            if (parameterDeclaration.Name == variable.Name)
            {
                if (parameterDeclaration.Children.FirstOrDefault(p => p is Identifier) is Identifier identifier)
                {
                    if (identifier.Name == variable.Name)
                    {
                        return identifier;
                    }
                }
                return node;
            }
        }
        if (node is VariableInitializer variableInitializer)
        {
            if (variableInitializer.Name == variable.Name)
            {
                return node;
            }
        }
        // foreach (var item in items), patterns (x is string text) and deconstruction
        if (node is SingleVariableDesignation designation)
        {
            if (designation.Identifier == variable.Name)
            {
                return node;
            }
        }
        // catch (Exception ex)
        if (node is CatchClause catchClause)
        {
            if (catchClause.VariableName == variable.Name)
            {
                return catchClause.VariableNameToken;
            }
        }

        foreach (var child in node.Children)
        {
            var usage = Find(child, variable);
            if (usage != null)
            {
                return usage;
            }
        }

        return null;
    }
}