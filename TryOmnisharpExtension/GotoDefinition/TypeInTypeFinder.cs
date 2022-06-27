using System.Linq;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition;

public class TypeInTypeFinder : IDefinitionInDecompiledSyntaxTreeFinder<ITypeDefinition>
{
    public AstNode Find(
        ITypeDefinition symbol,
        SyntaxTree rootTypeSyntaxTree)
    {
        var usage = Find(rootTypeSyntaxTree, symbol);

        return usage;
    }

    private AstNode Find(AstNode node, IEntity handleToSearchFor)
    {
        var symbol = node.GetSymbol();

        if (symbol != null)
        {
            if(symbol is IEntity entity)
            {
                if ( entity.AreSameUsingToken(handleToSearchFor) && node.NodeType == NodeType.TypeDeclaration)
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