using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;
public static class SymbolHelper
{
    public static bool AreSameUsingToken(this IEntity entity, IEntity toCompare)
    {
        if (entity == null || toCompare == null)
        {
            //TODO: Log what is going on here
            return false;
        }
        var sameToken = entity.MetadataToken == toCompare.MetadataToken;
        if (entity.ParentModule == null)
        {
            return false;
        }

        if (toCompare.ParentModule == null)
        {
            return false;
        }
        var sameAssembly = entity.ParentModule.AssemblyName == toCompare.ParentModule.AssemblyName;
        var result = sameAssembly && sameToken;
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