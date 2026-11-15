using CsDecompileLib.GotoDefinition;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.IlSpy.Ast;

public class EventNodeInTypeAstFinder : IDefinitionInDecompiledSyntaxTreeFinder<IEvent>
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
            // Only the declaration: the same event is also the symbol of every place that subscribes to it
            if (symbol is IEvent entity && node.IsMemberNode())
            {
                if (entity.AreSameUsingToken(eventEntityHandle))
                {
                    return node.FindNameIdentifier(symbol.Name);
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