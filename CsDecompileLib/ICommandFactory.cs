using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace CsDecompileLib;

public interface ICommandFactory<T> : IDecompilerCommandFactory<T>
{
    T GetForNamespace(string namespaceString);
    T GetForEnumField(IField field, string projectAssemblyFilePath);
    T GetForField(IField field, string projectAssemblyFilePath);
    T GetForInSource(Microsoft.CodeAnalysis.ISymbol roslynSymbol);
    T GetForFileNotFound(string filePath);
    T SymbolNotFoundAtLocation(string filePath, int line, int column);
    T GetForVariable(
        ILVariable variable,
        ITypeDefinition typeDefinition,
        AstNode methodNode,
        string sourceText,
        string assemblyFilePath);
}