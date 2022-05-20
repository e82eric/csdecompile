using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class MethodImplementedByAnalyzer
{
    private readonly AnalyzerScope _analyzerScope;

    [ImportingConstructor]
    public MethodImplementedByAnalyzer(AnalyzerScope analyzerScope)
    {
        _analyzerScope = analyzerScope;
    }
    
    public async Task<IEnumerable<IMethod>> Analyze(IMethod analyzedSymbol)
    {
        var result = new List<IMethod>();
        foreach (var type in await _analyzerScope.GetTypesInScope(analyzedSymbol))
        {
            foreach (var typeInScope in AnalyzeType((IMethod)analyzedSymbol, type))
            {
                result.Add(typeInScope);
            }
        }

        return result;
    }

    IEnumerable<IMethod> AnalyzeType(IMethod analyzedEntity, ITypeDefinition type)
    {
        var result = new List<IMethod>();
        var token = analyzedEntity.MetadataToken;
        var declaringTypeToken = analyzedEntity.DeclaringTypeDefinition.MetadataToken;
        var module = analyzedEntity.DeclaringTypeDefinition.ParentModule.PEFile;
        var allTypes = type.GetAllBaseTypeDefinitions();
        if (!allTypes.Any(t => t.MetadataToken == declaringTypeToken && t.ParentModule.PEFile == module))
            return result;

        foreach (var method in type.Methods)
        {
            var baseMembers = InheritanceHelper.GetBaseMembers(method, true);
            if (baseMembers.Any(m => m.MetadataToken == token && m.ParentModule.PEFile == module))
            {
                result.Add(method);
            }
        }

        return result;
    }
}