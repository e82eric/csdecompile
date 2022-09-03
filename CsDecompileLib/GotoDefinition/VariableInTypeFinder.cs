using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;

namespace CsDecompileLib.GotoDefinition;

public class VariableInTypeFinder
{
    public AstNode Find(
        ILVariable variableIdentifier,
        SyntaxTree rootTypeSyntaxTree)
    {
        var usage = Find(rootTypeSyntaxTree, variableIdentifier);

        return usage;
    }

    private AstNode Find(AstNode node, ILVariable variable)
    {
        if (node is VariableInitializer variableInitializer)
        {
            if (variableInitializer.Name == variable.Name)
            {
                return node;
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