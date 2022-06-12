using ICSharpCode.Decompiler.CSharp.Syntax;

namespace TryOmnisharpExtension.GotoDefinition;

public interface IDefinitionInDecompiledSyntaxTreeFinder<T>
{
    AstNode Find(
        T entity,
        SyntaxTree syntaxTree);
}