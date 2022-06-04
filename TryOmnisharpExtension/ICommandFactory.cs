using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;

namespace TryOmnisharpExtension;

public interface ICommandFactory<T> : IDecompilerCommandFactory<T>
{
    T GetForType(ITypeDefinition type, string projectAssemblyFilePath);
    T GetForMethod(IMethod method, string projectAssemblyFilePath);
    T GetForField(IField field, string projectAssemblyFilePath);
    T GetForProperty(IProperty property, string projectAssemblyFilePath);
    T GetForInSource(Microsoft.CodeAnalysis.ISymbol roslynSymbol);
    T GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath);
}