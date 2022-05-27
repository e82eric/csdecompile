using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class IlSpyPropertyImplementationFinder
{
    private readonly PropertyImplementedByAnalyzer _implementedByAnalyzer;

    [ImportingConstructor]
    public IlSpyPropertyImplementationFinder(
        PropertyImplementedByAnalyzer implementedByAnalyzer)
    {
        _implementedByAnalyzer = implementedByAnalyzer;
    }
    
    public async Task<IEnumerable<IlSpyMetadataSource2>> Run(IProperty symbol, string projectName)
    {
        var result = new List<IlSpyMetadataSource2>();

        var implementations = await _implementedByAnalyzer.Analyze(symbol);

        foreach (var implementation in implementations)
        {
            await AddToResult((ITypeDefinition)implementation.DeclaringType, implementation, symbol.Name, projectName, result);
        }

        return result;
    }

    private async Task AddToResult(
        ITypeDefinition symbol,
        IProperty property,
        string methodName,
        string projectName,
        IList<IlSpyMetadataSource2> result)
    {
        var parentType = SymbolHelper.FindContainingType(symbol);
        var assemblyFullName = symbol.ParentModule.PEFile.FullName;
        var dotNetVersion = symbol.ParentModule.PEFile.Metadata.MetadataVersion;
        var sourceText = $"{property.FullName} ({assemblyFullName}, .net: {dotNetVersion})";
        
        var assemblyVersion = symbol.ParentModule.PEFile.Metadata.GetAssemblyDefinition().Version.ToString();
        var typeFullName = GetTypeFullName(property);

        if (symbol.ParentModule.AssemblyName != projectName)
        {
            var metadataSource = new IlSpyMetadataSource2
            {
                AssemblyName = symbol.ParentModule.AssemblyName,
                SourceText = sourceText,
                ContainingTypeFullName = parentType.ReflectionName,
                AssemblyFilePath = symbol.Compilation.MainModule.PEFile.FileName,
                UsageType = UsageTypes.PropertyImplementation,
                NamespaceName = symbol.Namespace,
                TypeName = symbol.Name,
                MethodName = methodName,
                DotNetVersion = dotNetVersion,
                AssemblyVersion = assemblyVersion,
                TypeFullName = typeFullName
            };
            result.Add(metadataSource);
        }
    }
    
    private string GetTypeFullName(IProperty symbol)
    {
        var result = $"{symbol.DeclaringType.Name}.{symbol.Name}";
        var ittr = symbol.DeclaringType;
        
        while (ittr.DeclaringType != null)
        {
            result = $"{ittr.Name}+{result}";
            ittr = (ITypeDefinition)ittr.DeclaringType;
        }

        return result;
    }
}
