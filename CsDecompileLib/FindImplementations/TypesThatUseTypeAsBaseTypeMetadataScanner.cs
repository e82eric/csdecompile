﻿using System.Collections.Generic;
using CsDecompileLib.FindUsages;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindImplementations;

public class TypesThatUseTypeAsBaseTypeMetadataScanner : IMetadataUsagesScanner<ITypeDefinition>
{
    private readonly AnalyzerScope _analyzerScope;

    public TypesThatUseTypeAsBaseTypeMetadataScanner(AnalyzerScope analyzerScope)
    {
        _analyzerScope = analyzerScope;
    }

    public IEnumerable<ITypeDefinition> GetRootTypesThatUseSymbol(ITypeDefinition analyzedSymbol)
    {
        var types = _analyzerScope.GetTypesInScope(analyzedSymbol);

        var alreadyAddedParents = new HashSet<string>();
        var result = new List<ITypeDefinition>();
        foreach (var type in types)
        {
            var parentType = SymbolHelper.FindContainingType(type);
            if (!alreadyAddedParents.Contains(parentType.ReflectionName))
            {
                var usedByType = ScanType(analyzedSymbol, type);
                if (usedByType)
                {
                    result.Add(parentType);
                    alreadyAddedParents.Add(parentType.ReflectionName);
                }
            }
        }

        return result;
    }

    bool ScanType(ITypeDefinition analyzedEntity, ITypeDefinition type)
    {
        if (analyzedEntity.AreSameUsingToken(type))
        {
            return true;
        }

        foreach (var bt in type.DirectBaseTypes)
        {
            if (bt.FullName == analyzedEntity.FullName)
            {
                return true;
            }
        }

        return false;
    }
}