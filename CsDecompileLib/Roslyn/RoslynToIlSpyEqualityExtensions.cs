using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.TypeSystem.Implementation;
using Microsoft.CodeAnalysis;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;
using SymbolKind = ICSharpCode.Decompiler.TypeSystem.SymbolKind;
using TypeKind = ICSharpCode.Decompiler.TypeSystem.TypeKind;

namespace CsDecompileLib.Roslyn;

// Decides whether a Roslyn symbol and an ILSpy symbol are the same member.
//
// The comparison is done on the signature. Metadata tokens can't be used: Roslyn usually reads a
// reference assembly (System.Runtime, System.Console, ...) while ILSpy reads the implementation
// assembly, and the same token points at a different member in each of them.
public static class RoslynToIlSpyEqualityExtensions
{
    public static bool AreSameMethod(IMethodSymbol roslynMethodSymbol, IMethod ilSpySymbol)
    {
        if (roslynMethodSymbol == null || ilSpySymbol == null)
        {
            return false;
        }

        // list.Where(...) is seen by Roslyn as a method without the "this" parameter, and
        // List<int>.Add(int) as a method taking int. The declaration is what exists in metadata.
        var roslynMethod = (roslynMethodSymbol.ReducedFrom ?? roslynMethodSymbol).OriginalDefinition;

        if (roslynMethod.MetadataName != ilSpySymbol.Name)
        {
            return false;
        }

        if (roslynMethod.Arity != ilSpySymbol.TypeParameters.Count)
        {
            return false;
        }

        if (!AreSameParameters(roslynMethod.Parameters, ilSpySymbol.Parameters))
        {
            return false;
        }

        // Conversion operators are the only methods that overload on the return type.
        if (roslynMethod.MethodKind == MethodKind.Conversion &&
            !AreSameType(roslynMethod.ReturnType, ilSpySymbol.ReturnType))
        {
            return false;
        }

        return true;
    }

    public static bool AreSameProperty(IPropertySymbol roslynPropertySymbol, IProperty ilSpySymbol)
    {
        if (roslynPropertySymbol == null || ilSpySymbol == null)
        {
            return false;
        }

        var roslynProperty = roslynPropertySymbol.OriginalDefinition;
        if (roslynProperty.MetadataName != ilSpySymbol.Name)
        {
            return false;
        }

        // Only indexers have parameters, and they can be overloaded on them.
        return AreSameParameters(roslynProperty.Parameters, ilSpySymbol.Parameters);
    }

    //This works for fields and events, which can't be overloaded.  Methods and properties have their own method
    public static bool AreMemberSymbol(ISymbol roslynSymbol, IMember ilSpySymbol)
    {
        if (roslynSymbol == null || ilSpySymbol == null)
        {
            return false;
        }

        return roslynSymbol.MetadataName == ilSpySymbol.Name;
    }

    private static bool AreSameParameters(
        IReadOnlyList<IParameterSymbol> roslynParameters,
        IReadOnlyList<IParameter> ilSpyParameters)
    {
        if (roslynParameters.Count != ilSpyParameters.Count)
        {
            return false;
        }

        for (int i = 0; i < roslynParameters.Count; i++)
        {
            var ilSpyParameterType = ilSpyParameters[i].Type;
            var ilSpyIsByRef = ilSpyParameterType is ByReferenceType;
            var roslynIsByRef = roslynParameters[i].RefKind != RefKind.None;
            if (ilSpyIsByRef != roslynIsByRef)
            {
                return false;
            }

            if (ilSpyParameterType is ByReferenceType byReferenceType)
            {
                ilSpyParameterType = byReferenceType.ElementType;
            }

            if (!AreSameType(roslynParameters[i].Type, ilSpyParameterType))
            {
                return false;
            }
        }

        return true;
    }

    public static bool AreSameType(ITypeSymbol roslynType, IType ilSpyType)
    {
        if (roslynType == null || ilSpyType == null)
        {
            return false;
        }

        ilSpyType = RemoveDecorations(ilSpyType);

        switch (roslynType)
        {
            case IArrayTypeSymbol roslynArray:
                return ilSpyType is ArrayType ilSpyArray &&
                       roslynArray.Rank == ilSpyArray.Dimensions &&
                       AreSameType(roslynArray.ElementType, ilSpyArray.ElementType);

            case IPointerTypeSymbol roslynPointer:
                return ilSpyType is PointerType ilSpyPointer &&
                       AreSameType(roslynPointer.PointedAtType, ilSpyPointer.ElementType);

            case IFunctionPointerTypeSymbol:
                return ilSpyType.Kind == TypeKind.FunctionPointer;

            case IDynamicTypeSymbol:
                return ilSpyType.Kind == TypeKind.Dynamic || ilSpyType.ReflectionName == "System.Object";

            case ITypeParameterSymbol roslynTypeParameter:
                return ilSpyType is ITypeParameter ilSpyTypeParameter &&
                       AreSameTypeParameter(roslynTypeParameter, ilSpyTypeParameter);

            case INamedTypeSymbol roslynNamedType:
                return AreSameNamedType(roslynNamedType, ilSpyType);

            default:
                return false;
        }
    }

    private static bool AreSameNamedType(INamedTypeSymbol roslynNamedType, IType ilSpyType)
    {
        var ilSpyDefinition = ilSpyType.GetDefinition();
        if (ilSpyDefinition == null)
        {
            // dynamic is System.Object in metadata
            return ilSpyType.Kind == TypeKind.Dynamic && roslynNamedType.SpecialType == Microsoft.CodeAnalysis.SpecialType.System_Object;
        }

        if (roslynNamedType.OriginalDefinition.GetReflectionName() != ilSpyDefinition.ReflectionName)
        {
            return false;
        }

        // ILSpy lists the type arguments of the outer types as well: Outer<int>.Inner<string> has two.
        var roslynTypeArguments = GetAllTypeArguments(roslynNamedType);
        var ilSpyTypeArguments = ilSpyType.TypeArguments;
        if (roslynTypeArguments.Count != ilSpyTypeArguments.Count)
        {
            return false;
        }

        for (int i = 0; i < roslynTypeArguments.Count; i++)
        {
            if (!AreSameType(roslynTypeArguments[i], ilSpyTypeArguments[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreSameTypeParameter(ITypeParameterSymbol roslynTypeParameter, ITypeParameter ilSpyTypeParameter)
    {
        var roslynIsMethodParameter = roslynTypeParameter.TypeParameterKind == TypeParameterKind.Method;
        var ilSpyIsMethodParameter = ilSpyTypeParameter.OwnerType == SymbolKind.Method;
        if (roslynIsMethodParameter != ilSpyIsMethodParameter)
        {
            return false;
        }

        var roslynIndex = roslynTypeParameter.Ordinal;
        if (!roslynIsMethodParameter)
        {
            // Roslyn counts within the declaring type, ILSpy counts the outer types' parameters too.
            var outerType = roslynTypeParameter.ContainingType?.ContainingType;
            while (outerType != null)
            {
                roslynIndex += outerType.TypeParameters.Length;
                outerType = outerType.ContainingType;
            }
        }

        return roslynIndex == ilSpyTypeParameter.Index;
    }

    private static List<ITypeSymbol> GetAllTypeArguments(INamedTypeSymbol roslynNamedType)
    {
        var chain = new Stack<INamedTypeSymbol>();
        for (var current = roslynNamedType; current != null; current = current.ContainingType)
        {
            chain.Push(current);
        }

        return chain.SelectMany(type => type.TypeArguments).ToList();
    }

    // Nullable reference annotations, tuple element names and modifiers don't change which type it is.
    private static IType RemoveDecorations(IType ilSpyType)
    {
        while (true)
        {
            switch (ilSpyType)
            {
                case NullabilityAnnotatedType annotated:
                    ilSpyType = annotated.TypeWithoutAnnotation;
                    continue;
                case TupleType tuple:
                    ilSpyType = tuple.UnderlyingType;
                    continue;
                case ModifiedType modified:
                    ilSpyType = modified.ElementType;
                    continue;
                default:
                    return ilSpyType;
            }
        }
    }
}
