using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;

namespace TryOmnisharpExtension;

public interface ICommandFactory<T>
{
    T GetForType(ITypeDefinition type, string projectAssemblyFilePath);
    T GetForMethod(IMethod method, string projectAssemblyFilePath);
    T GetForProperty(IProperty property, string projectAssemblyFilePath);
    T GetForInSource(Microsoft.CodeAnalysis.ISymbol roslynSymbol);
}