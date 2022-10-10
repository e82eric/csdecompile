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
                if (entity.AreSameUsingToken(handleToSearchFor) && node.NodeType == NodeType.Member)
                {
                    var identifier = node.Children.Where(n =>
                    {
                        var entity = n as Identifier;
                        if (entity != null)
                        {
                            if (entity.Name == symbol.Name)
                            {
                                return true;
                            }
                        }

                        return false;
                    }).FirstOrDefault();

                    if (identifier != null)
                    {
                        return identifier;
                    }
                    else
                    {
                        return node;
                    }
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