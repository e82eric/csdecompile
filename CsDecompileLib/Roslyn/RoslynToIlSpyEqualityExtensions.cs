﻿using System;
using System.Linq;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace CsDecompileLib.Roslyn;

public static class RoslynToIlSpyEqualityExtensions
{
    public static bool AreSameMethod(IMethodSymbol roslynMethodSymbol, IMethod ilSpySymbol)
    {
        var areSameUsingToken = AreSameUsingToken(roslynMethodSymbol, ilSpySymbol);
        if (areSameUsingToken)
        {
            return true;
        }
        
        if (roslynMethodSymbol.Name != ilSpySymbol.Name)
        {
            return false;
        }

        if (roslynMethodSymbol.Parameters.Count() != ilSpySymbol.Parameters.Count)
        {
            return false;
        }

        for (int i = 0; i < roslynMethodSymbol.Parameters.Count(); i++)
        {
            var roslynParameterType = roslynMethodSymbol.Parameters[i].Type;
            var ilSpyParameterTYpe = ilSpySymbol.Parameters[i].Type;

            var sameType = AreSameType(roslynParameterType, ilSpyParameterTYpe);
            if (!sameType)
            {
                return false;
            }
        }

        return true;
    }
    
    //This doesn't work for BCL libraries.
    //Ilspy uses System.private.corelib Roslyn uses System.Runtime for the assembly name
    //And since the assemblies are different you get a different token value (you can't do the comparison on namespace)
    //This can be done as an initial check and then fallback to check stuff using names
    private static bool AreSameUsingToken(ISymbol roslynSymbol, IEntity ilSpySymbol)
    {
        var sameAssembly = roslynSymbol.ContainingAssembly.Name == ilSpySymbol.ParentModule.AssemblyName;
        if (!sameAssembly)
        {
            return false;
        }
        var roslynEntityHandle = MetadataTokenHelpers.EntityHandleOrNil(roslynSymbol.MetadataToken);
        var sameHandle = roslynEntityHandle == ilSpySymbol.MetadataToken;
        return sameHandle;
    }
    
    //This works for properties and fields.  Methods should be checked using its method
    public static bool AreMemberSymbol(ISymbol roslynSymbol, IMember ilSpySymbol)
    {
        var areSameUsingToken = AreSameUsingToken(roslynSymbol, ilSpySymbol);
        if (areSameUsingToken)
        {
            return true;
        }
        
        if (roslynSymbol.Name != ilSpySymbol.Name)
        {
            return false;
        }

        return true;
    }

    private static bool AreSameType(ITypeSymbol roslynSymbol, ITypeDefinition ilSpySymbol)
    {
        var areSameUsingToken = AreSameUsingToken(roslynSymbol, ilSpySymbol);
        if (areSameUsingToken)
        {
            return true;
        }
        
        var roslynNamespace = roslynSymbol.ContainingNamespace.ToDisplayString();
        if (roslynNamespace != ilSpySymbol.Namespace)
        {
            return false;
        }

        if (roslynSymbol.Name != ilSpySymbol.Name)
        {
            return false;
        }
        
        if(roslynSymbol is INamedTypeSymbol roslynNamedType)
        {
            if (roslynNamedType.TypeArguments.Length != ilSpySymbol.TypeArguments.Count)
            {
                return false;
            }

            for (int i = 0; i < roslynNamedType.TypeArguments.Length; i++)
            {
                if (!(roslynNamedType.TypeArguments[i].TypeKind == TypeKind.TypeParameter &&
                    ilSpySymbol.TypeArguments[i].Kind == ICSharpCode.Decompiler.TypeSystem.TypeKind.TypeParameter))
                {
                    var areEqual = AreSameType(
                        roslynNamedType.TypeArguments[i],
                        ilSpySymbol.TypeArguments[i].GetDefinition());
                    if (!areEqual)
                    {
                        return false;
                    }
                }
            }
        }
        
        return true;
    }

    private static bool AreSameType(ITypeSymbol roslynSymbol, IType ilSpySymbol)
    {
        bool ilSpyIsByRef = false;
        IType ilSpyType = ilSpySymbol;
        if (ilSpySymbol is ByReferenceType byReferenceType)
        {
            ilSpyIsByRef = true;
            ilSpyType = byReferenceType.ElementType;
        }

        ITypeDefinition typeDefinition = ilSpyType.GetDefinition();

        if (typeDefinition != null)
        {
            var result = AreSameType(roslynSymbol, typeDefinition);
            return result;
        }

        if(roslynSymbol.TypeKind == TypeKind.TypeParameter)
        {
            if (ilSpyIsByRef)
            {
                //There has to be better way to do this
                if (roslynSymbol.Name + "&" == ilSpySymbol.FullName)
                {
                    return true;
                }
            }
            else
            {
                if (roslynSymbol.Name == ilSpySymbol.FullName)
                {
                    return true;
                }
            }
        }

        return false;
    }
}