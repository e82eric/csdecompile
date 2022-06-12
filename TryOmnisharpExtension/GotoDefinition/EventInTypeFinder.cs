using System.Composition;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition;

[Export]
public class EventInTypeFinder : IDefinitionInDecompiledSyntaxTreeFinder<IEvent>
{
    public AstNode Find(
        IEvent @event,
        SyntaxTree rootTypeSyntaxTree)
    {
        var result = Find(rootTypeSyntaxTree, @event.MetadataToken);

        return result;
    }

    private AstNode Find(AstNode node, EntityHandle? eventEntityHandle)
    {
        var symbol = node.GetSymbol();

        if (symbol != null)
        {
            if (symbol is IEvent entity)
            {
                if (entity.MetadataToken == eventEntityHandle)
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