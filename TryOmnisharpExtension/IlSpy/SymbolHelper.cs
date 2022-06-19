using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;
public static class SymbolHelper
{
    public static bool AreSameUsingToken(this IEntity entity, IEntity toCompare)
    {
        var result = entity.ParentModule.AssemblyName == toCompare.ParentModule.AssemblyName
             && entity.MetadataToken == toCompare.MetadataToken;
        return result;
    }
    public static ITypeDefinition FindContainingType(IEntity symbol)
    {
        if (symbol.DeclaringType == null)
        {
            return symbol as ITypeDefinition;
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