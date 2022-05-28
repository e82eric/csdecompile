using System.Linq;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;

namespace TryOmnisharpExtension;

public static class RoslynToIlSpyEqualityExtensions
{
    public static bool AreSameMethod(IMethodSymbol roslynMethodSymbol, IMethod ilSpySymbol)
    {
        if (roslynMethodSymbol.Name != ilSpySymbol.Name)
        {
            return false;
        }

        if (roslynMethodSymbol.Parameters.Count() != ilSpySymbol.Parameters.Count())
        {
            return false;
        }

        for (int i = 0; i < roslynMethodSymbol.Parameters.Count(); i++)
        {
            var sameType = AreSameType(roslynMethodSymbol.Parameters[i].Type, ilSpySymbol.Parameters[i].Type);
            if (!sameType)
            {
                return false;
            }
        }

        return true;
    }

    public static bool AreSameType(ITypeSymbol roslynTypeSymbol, IType ilSpyTypeSymbol)
    {
        if (roslynTypeSymbol.ContainingNamespace.Name != ilSpyTypeSymbol.Namespace)
        {
            return false;
        }

        if (roslynTypeSymbol.Name != ilSpyTypeSymbol.Name)
        {
            return false;
        }
        
        if(roslynTypeSymbol is INamedTypeSymbol roslynNamedType)
        {
            if (roslynNamedType.TypeParameters.Length != ilSpyTypeSymbol.TypeParameterCount)
            {
                return false;
            }

            for (int i = 0; i < roslynNamedType.TypeParameters.Length; i++)
            {
                var areEqual = AreSameType(roslynNamedType.TypeParameters[i], ilSpyTypeSymbol.TypeParameters[i]);
                if (!areEqual)
                {
                    return false;
                }
            }
        }
        
        return true;
    }
}