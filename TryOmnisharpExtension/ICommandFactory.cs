using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;

namespace TryOmnisharpExtension;

public interface ICommandFactory<T> : IDecompilerCommandFactory<T>
{
    T GetForField(IField field, string projectAssemblyFilePath);
    T GetForInSource(Microsoft.CodeAnalysis.ISymbol roslynSymbol);
}