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
    
    public static bool AreSameField(IFieldSymbol roslynSymbol, IField ilSpySymbol)
    {
        if (roslynSymbol.Name != ilSpySymbol.Name)
        {
            return false;
        }

        return true;
    }

    public static bool AreSameType(ITypeSymbol roslynTypeSymbol, IType ilSpyTypeSymbol)
    {
        var roslynNamespace = roslynTypeSymbol.ContainingNamespace.ToDisplayString();
        if (roslynNamespace != ilSpyTypeSymbol.Namespace)
        {
            return false;
        }

        if (roslynTypeSymbol.Name != ilSpyTypeSymbol.Name)
        {
            return false;
        }
        
        if(roslynTypeSymbol is INamedTypeSymbol roslynNamedType)
        {
            if (roslynNamedType.TypeArguments.Length != ilSpyTypeSymbol.TypeArguments.Count)
            {
                return false;
            }

            for (int i = 0; i < roslynNamedType.TypeArguments.Length; i++)
            {
                var areEqual = AreSameType(roslynNamedType.TypeArguments[i], ilSpyTypeSymbol.TypeArguments[i]);
                if (!areEqual)
                {
                    return false;
                }
            }
        }
        
        return true;
    }
}