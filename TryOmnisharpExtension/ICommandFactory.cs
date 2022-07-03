using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using TryOmnisharpExtension.GotoDefinition;
using SyntaxTree = ICSharpCode.Decompiler.CSharp.Syntax.SyntaxTree;

namespace TryOmnisharpExtension;

public interface ICommandFactory<T> : IDecompilerCommandFactory<T>
{
    T GetForField(IField field, string projectAssemblyFilePath);
    T GetForInSource(Microsoft.CodeAnalysis.ISymbol roslynSymbol);
    T GetForFileNotFound(string filePath);
    T SymbolNotFoundAtLocation(string filePath, int line, int column);
    T GetForVariable(
        ILVariable variable,
        ITypeDefinition typeDefinition,
        SyntaxTree syntaxTree,
        string sourceText,
        string assemblyFilePath);
}