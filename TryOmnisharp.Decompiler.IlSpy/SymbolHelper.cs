using ICSharpCode.Decompiler.TypeSystem;

namespace IlSpy.Analyzer.Extraction;

public static class SymbolHelper
{
    public static ITypeDefinition FindContainingType(ITypeDefinition symbol)
    {
        if (symbol.DeclaringType == null)
        {
            return symbol;
        }
        IType result = symbol.DeclaringType;

        while (result.DeclaringType != null)
        {
            result = result.DeclaringType;
        }

        if (result is ParameterizedType)
        {
            return null;
        }

        return (ITypeDefinition)result;
    }
}