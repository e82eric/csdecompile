using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib;

public interface IDecompilerCommandFactory<T>
{
    T GetForType(ITypeDefinition type, string projectAssemblyFilePath);
    T GetForMethod(IMethod method, string projectAssemblyFilePath);
    T GetForProperty(IProperty property, string projectAssemblyFilePath);
    T GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath);
}