using System.Collections.Generic;
using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.FindUsages;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindImplementations;

[Export]
public class TypesThatUseTypeAsBaseTypeMetadataScanner : IMetadataUsagesScanner<ITypeDefinition>
{
    private readonly AnalyzerScope _analyzerScope;

    [ImportingConstructor]
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

    bool ScanType(ITypeDefinition analyzedEntity, ITypeDefinition type)
    {
        if (analyzedEntity.ParentModule.PEFile == type.ParentModule.PEFile
            && analyzedEntity.MetadataToken == type.MetadataToken)
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