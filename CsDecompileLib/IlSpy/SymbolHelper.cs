using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.IlSpy;
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

        if (result is ParameterizedType parameterizedType)
        {
            return parameterizedType.GenericType.GetDefinition();
        }

        return (ITypeDefinition)result;
    }

    public static bool AreSameMethodSignature(IMethod method1, IMethod method2)
    {
        if (method1.Name == method2.Name)
        {
            if (method1.Parameters.Count == method2.Parameters.Count)
            {
                bool allParametersMatch = true;
                for (int i = 0; i < method1.Parameters.Count; i++)
                {
                    if (method1.Parameters[i].Type.FullName != method2.Parameters[i].Type.FullName)
                    {
                        allParametersMatch = false;
                        break;
                    }
                }

                if (allParametersMatch)
                {
                    if (method1.TypeParameters.Count == method2.TypeParameters.Count)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}