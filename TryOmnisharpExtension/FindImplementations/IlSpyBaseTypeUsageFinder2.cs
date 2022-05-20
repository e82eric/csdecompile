using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharp.Decompiler.IlSpy2;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class IlSpyBaseTypeUsageFinder2
{
    private readonly TypeUsedByAnalyzer2 _typeUsedByAnalyzer;
    private readonly TypeUsedInTypeFinder _typeUsedInTypeFinder;

    [ImportingConstructor]
    public IlSpyBaseTypeUsageFinder2(
        TypeUsedByAnalyzer2 typeUsedByAnalyzer,
        TypeUsedInTypeFinder typeUsedInTypeFinder)
    {
        _typeUsedByAnalyzer = typeUsedByAnalyzer;
        _typeUsedInTypeFinder = typeUsedInTypeFinder;
    }
    
    public async Task<IEnumerable<IlSpyMetadataSource2>> Run(ITypeDefinition typeDefinition, string projectAssemblyFilePath)
    {
        var result = new List<IlSpyMetadataSource2>();
        
        var typeUsedByResult = await _typeUsedByAnalyzer.Analyze(typeDefinition);

        var types = typeUsedByResult.Where(r => r.SymbolKind == SymbolKind.TypeDefinition);

        foreach (var type in types)
        {
            await AddTypeDefinitionToResult((ITypeDefinition)type, result, typeDefinition);
        }

        return result;
    }

    private async Task AddTypeDefinitionToResult(ITypeDefinition symbol, IList<IlSpyMetadataSource2> result, IEntity typeToSearchFor)
    {
        var parentType = SymbolHelper.FindContainingType(symbol);

        var usagesInTypeDefintions = await _typeUsedInTypeFinder.Find2(parentType, typeToSearchFor.MetadataToken);

        foreach (var usage in usagesInTypeDefintions)
        {
            var metadataSource = new IlSpyMetadataSource2
            {
                AssemblyName = symbol.ParentModule.AssemblyName,
                Column = usage.StartLocation.Column,
                Line = usage.StartLocation.Line,
                SourceText = symbol.ReflectionName,
                StartColumn = usage.StartLocation.Column,
                EndColumn = usage.EndLocation.Column,
                ContainingTypeFullName = parentType.ReflectionName,
                AssemblyFilePath = symbol.Compilation.MainModule.PEFile.FileName
            };

            result.Add(metadataSource);
        }
    }
}