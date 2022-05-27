using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharp.Decompiler.IlSpy2;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class IlSpyBaseTypeUsageFinder2
{
    private readonly TypeUsedByAnalyzer2 _typeUsedByAnalyzer;

    [ImportingConstructor]
    public IlSpyBaseTypeUsageFinder2(
        TypeUsedByAnalyzer2 typeUsedByAnalyzer)
    {
        _typeUsedByAnalyzer = typeUsedByAnalyzer;
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

        var assemblyVersion = symbol.ParentModule.PEFile.Metadata.GetAssemblyDefinition().Version.ToString();
        var assemblyFullName = symbol.ParentModule.PEFile.FullName;
        var dotNetVersion = symbol.ParentModule.PEFile.Metadata.MetadataVersion;
        var typeFullName = GetTypeFullName(symbol);

        var sourceType = $"{symbol.ReflectionName} ({assemblyFullName}, .net: {dotNetVersion})";

        var metadataSource = new IlSpyMetadataSource2
        {
            AssemblyName = symbol.ParentModule.AssemblyName,
            SourceText = sourceType,
            AssemblyFilePath = symbol.Compilation.MainModule.PEFile.FileName,
            UsageType = UsageTypes.BaseType,
            NamespaceName = symbol.Namespace,
            ContainingTypeFullName = parentType.ReflectionName,
            TypeName = symbol.Name,
            BaseTypeName = typeToSearchFor.Name,
            DotNetVersion = dotNetVersion,
            AssemblyVersion = assemblyVersion,
            TypeFullName = typeFullName
        };

        result.Add(metadataSource);
    }

    private string GetTypeFullName(ITypeDefinition typeDefinition)
    {
        var result = typeDefinition.Name;
        var ittr = typeDefinition;
        
        while (ittr.DeclaringType != null)
        {
            ittr = (ITypeDefinition)ittr.DeclaringType;
            result = $"{ittr.Name}+{result}";
        }

        return result;
    }
}