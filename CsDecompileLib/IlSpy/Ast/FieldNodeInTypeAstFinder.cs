using System.Linq;
using CsDecompileLib.GotoDefinition;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.IlSpy.Ast;

public class FieldNodeInTypeAstFinder : IDefinitionInDecompiledSyntaxTreeFinder<IField>
{
    public AstNode Find(
        IField field,
        SyntaxTree rootTypeSyntaxTree)
    {
        var usage = Find(rootTypeSyntaxTree, field);

        return usage;
    }

    private AstNode Find(AstNode node, IEntity handleToSearchFor)
    {
        var symbol = node.GetSymbol();

        if (symbol != null)
        {
            if(symbol is IField entity)
            {
                if (entity.AreSameUsingToken(handleToSearchFor) && node.IsMemberNode())
                {
                    return node.FindNameIdentifier(symbol.Name);
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