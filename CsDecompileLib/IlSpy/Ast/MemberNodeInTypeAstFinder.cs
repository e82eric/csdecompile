using CsDecompileLib.GotoDefinition;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.IlSpy.Ast;

public class MemberNodeInTypeAstFinder : IDefinitionInDecompiledSyntaxTreeFinder<IMember>
{
    public AstNode Find(
        IMember handleToSearchFor,
        SyntaxTree rootTypeSyntaxTree)
    {
        var usage = Find(rootTypeSyntaxTree, handleToSearchFor);
        return usage;
    }

    private AstNode Find(AstNode node, IEntity handleToSearchFor)
    {
        var symbol = node.GetSymbol();

        if (symbol != null)
        {
            if(symbol is IEntity entity)
            {
                if (entity.AreSameUsingToken(handleToSearchFor) && node.NodeType == NodeType.Member)
                {
                    return node;
                }
            }
        }

        foreach (var child in node.Children)
        {
            var usage = Find(child, handleToSearchFor);
            if (usage != null)
            {
                return usage;
            }
        }

        return null;
    }
}