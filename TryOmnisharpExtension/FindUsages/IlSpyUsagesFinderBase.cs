using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages;

[Export]
public class IlSpyUsagesFinderBase<T>
{
    private readonly DecompilerFactory _decompilerFactory;
    private readonly IMetadataUsagesScanner<T> _typeUsedByTypeIlScanner;
    private readonly IEntityUsedInTypeFinder<T> _typeUsedInTypeFinder2;

    [ImportingConstructor]
    public IlSpyUsagesFinderBase(
        DecompilerFactory decompilerFactory,
        IMetadataUsagesScanner<T> typeUsedByTypeIlScanner,
        IEntityUsedInTypeFinder<T> typeUsedInTypeFinder2)
    {
        _typeUsedInTypeFinder2 = typeUsedInTypeFinder2;
        _typeUsedByTypeIlScanner = typeUsedByTypeIlScanner;
        _decompilerFactory = decompilerFactory;
    }
        
    public IEnumerable<IlSpyMetadataSource2> Run(T symbol)
    {
        var result = new List<IlSpyMetadataSource2>();

        var typesThatUseType = _typeUsedByTypeIlScanner.GetRootTypesThatUseSymbol(symbol);
            
        var decompiledTypeDefintions = new ConcurrentDictionary<string, (SyntaxTree, string)>();
        Parallel.ForEach(typesThatUseType, new ParallelOptions {MaxDegreeOfParallelism = 25} , typeToDecompile =>
        {
            var decompiledTypeDefinition = DecompileTypeDefinition(typeToDecompile);
            decompiledTypeDefintions.TryAdd(typeToDecompile.FullName, decompiledTypeDefinition);
        });

        foreach (var typeToSearch in typesThatUseType)
        {
            if (decompiledTypeDefintions.TryGetValue(
                    typeToSearch.FullName,
                    out (SyntaxTree syntaxTree, string sourceText) decompiledParentType))
            {
                var usages = _typeUsedInTypeFinder2.Find(
                    decompiledParentType,
                    typeToSearch,
                    symbol);
                    
                foreach (var usage in usages)
                {
                    var metadataSource = new IlSpyMetadataSource2
                    {
                        AssemblyName = typeToSearch.ParentModule.AssemblyName,
                        Column = usage.StartLocation.Column,
                        Line = usage.StartLocation.Line,
                        SourceText = usage.Statement,
                        StartColumn = usage.StartLocation.Column,
                        EndColumn = usage.EndLocation.Column,
                        ContainingTypeFullName = typeToSearch.ReflectionName,
                        AssemblyFilePath = typeToSearch.Compilation.MainModule.PEFile.FileName,
                        UsageType = UsageTypes.Type,
                        TypeName = typeToSearch.ReflectionName,
                        NamespaceName = typeToSearch.Namespace
                    };
                    result.Add(metadataSource);
                }
            }
        }
                
        return result;
    }

    private (SyntaxTree, string) DecompileTypeDefinition(ITypeDefinition typeDefinition)
    {
        var fileName = typeDefinition.Compilation.MainModule.PEFile.FileName;
        var cachingDecompiler = _decompilerFactory.Get(fileName);

        var (syntaxTree, sourceText) = cachingDecompiler.Run(typeDefinition);
        return (syntaxTree, sourceText);
    }
}