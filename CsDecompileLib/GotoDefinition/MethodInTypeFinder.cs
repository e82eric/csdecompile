using System.Linq;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using CsDecompileLib.IlSpy;

namespace CsDecompileLib.GotoDefinition;

public class MethodInTypeFinder : IDefinitionInDecompiledSyntaxTreeFinder<IMethod>
{
    public AstNode Find(
        IMethod handleToSearchFor,
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