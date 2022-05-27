using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class PropertyImplementedByAnalyzer
{
    private readonly AnalyzerScope _analyzerScope;

    [ImportingConstructor]
    public PropertyImplementedByAnalyzer(AnalyzerScope analyzerScope)
    {
        _analyzerScope = analyzerScope;
    }
    
    public async Task<IEnumerable<IProperty>> Analyze(IProperty analyzedSymbol)
    {
        var result = new List<IProperty>();
        foreach (var type in await _analyzerScope.GetTypesInScope(analyzedSymbol))
        {
            foreach (var typeInScope in AnalyzeType(analyzedSymbol, type))
            {
                result.Add(typeInScope);
            }
        }

        return result;
    }

    IEnumerable<IProperty> AnalyzeType(IProperty analyzedEntity, ITypeDefinition type)
    {
        var result = new List<IProperty>();
        var token = analyzedEntity.MetadataToken;
        var declaringTypeToken = analyzedEntity.DeclaringTypeDefinition.MetadataToken;
        var module = analyzedEntity.DeclaringTypeDefinition.ParentModule.PEFile;
        var allTypes = type.GetAllBaseTypeDefinitions();
        if (!allTypes.Any(t => t.MetadataToken == declaringTypeToken && t.ParentModule.PEFile == module))
        {
            return result;
        }

        foreach (var property in type.Properties)
        {
            var baseMembers = InheritanceHelper.GetBaseMembers(property, true);
            if (baseMembers.Any(m => m.MetadataToken == token && m.ParentModule.PEFile == module))
            {
                result.Add(property);
            }
        }

        return result;
    }
}