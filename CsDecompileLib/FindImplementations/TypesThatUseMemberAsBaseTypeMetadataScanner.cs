﻿using System.Collections.Generic;
using CsDecompileLib.FindUsages;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindImplementations;

public class TypesThatUseMemberAsBaseTypeMetadataScanner : IMetadataUsagesScanner<IMember>
{
    private readonly AnalyzerScope _analyzerScope;

    public TypesThatUseMemberAsBaseTypeMetadataScanner(AnalyzerScope analyzerScope)
    {
        _analyzerScope = analyzerScope;
    }

    public IEnumerable<ITypeDefinition> GetRootTypesThatUseSymbol(IMember analyzedSymbol)
    {
        if (!analyzedSymbol.IsOverridable)
        {
            return new ITypeDefinition[] { };
        }
        
        var types = _analyzerScope.GetTypesInScope(analyzedSymbol);

        var alreadyAddedParents = new HashSet<string>();
        var result = new List<ITypeDefinition>();
        foreach (var type in types)
        {
            var parentType = SymbolHelper.FindContainingType(type);
            if (!alreadyAddedParents.Contains(parentType.FullName))
            {
                var usedByType = ScanType(analyzedSymbol, type);
                if (usedByType)
                {
                    result.Add(parentType);
                    alreadyAddedParents.Add(parentType.FullName);
                }
            }
        }

        return result;
    }

    bool ScanType(IMember analyzedEntity, ITypeDefinition type)
    {
        foreach (var typeMember in type.Members)
        {
            if (typeMember.Name == analyzedEntity.Name)
            {
                var entityBaseMembers = InheritanceHelper.GetBaseMembers(typeMember, true);
                foreach (var entityBaseMember in entityBaseMembers)
                {
                    if (entityBaseMember.AreSameUsingToken(analyzedEntity))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}