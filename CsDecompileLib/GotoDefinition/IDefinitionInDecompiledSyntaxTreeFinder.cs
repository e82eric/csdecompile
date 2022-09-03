using ICSharpCode.Decompiler.CSharp.Syntax;

namespace CsDecompileLib.GotoDefinition;

public interface IDefinitionInDecompiledSyntaxTreeFinder<T>
{
    AstNode Find(
        T entity,
        SyntaxTree syntaxTree);
}