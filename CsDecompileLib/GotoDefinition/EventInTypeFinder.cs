using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using CsDecompileLib.IlSpy;

namespace CsDecompileLib.GotoDefinition;

public class EventInTypeFinder : IDefinitionInDecompiledSyntaxTreeFinder<IEvent>
{
    public AstNode Find(
        IEvent @event,
        SyntaxTree rootTypeSyntaxTree)
    {
        var result = Find(rootTypeSyntaxTree, @event);

        return result;
    }

    private AstNode Find(AstNode node, IEntity eventEntityHandle)
    {
        var symbol = node.GetSymbol();

        if (symbol != null)
        {
            if (symbol is IEvent entity)
            {
                if (entity.AreSameUsingToken(eventEntityHandle))
                {
                    return node;
                }
            }
        }

        foreach (var child in node.Children)
        {
            var usage = Find(child, eventEntityHandle);
            if (usage != null)
            {
                return usage;
            }
        }

        return null;
    }
}