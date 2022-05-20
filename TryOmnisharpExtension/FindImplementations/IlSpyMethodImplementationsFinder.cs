using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class IlSpyMethodImplementationFinder
{
    private readonly MethodImplementedByAnalyzer _methodImplementedByAnalyzer;
    private readonly MethodFinder _methodFinder;

    [ImportingConstructor]
    public IlSpyMethodImplementationFinder(
        MethodImplementedByAnalyzer methodImplementedByAnalyzer,
        MethodFinder methodFinder)
    {
        _methodImplementedByAnalyzer = methodImplementedByAnalyzer;
        _methodFinder = methodFinder;
    }
    
    public async Task<IEnumerable<IlSpyMetadataSource2>> Run(IMethod method, string projectName)
    {
        var result = new List<IlSpyMetadataSource2>();

        var implementations = await _methodImplementedByAnalyzer.Analyze(method);

        foreach (var implementation in implementations)
        {
            await AddTypeDefinitionToResult((ITypeDefinition)implementation.DeclaringType, implementation, projectName, result);
        }

        return result;
    }

    private async Task AddTypeDefinitionToResult(
        ITypeDefinition symbol,
        IMethod method,
        string projectName,
        IList<IlSpyMetadataSource2> result)
    {
        var parentType = SymbolHelper.FindContainingType(symbol);

        if (symbol.ParentModule.AssemblyName != projectName)
        {
            var usage = await _methodFinder.Find(method, parentType.MetadataToken, parentType);

            if (usage != null)
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
}