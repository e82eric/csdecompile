﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindUsages;

public class IlSpyUsagesFinderBase<T>: IlSpyToSourceInfoBase
{
    private readonly DecompilerFactory _decompilerFactory;
    private readonly IMetadataUsagesScanner<T> _typeUsedByTypeIlScanner;
    private readonly IEntityUsedInTypeFinder<T> _usageFinder;

    public IlSpyUsagesFinderBase(
        DecompilerFactory decompilerFactory,
        IMetadataUsagesScanner<T> typeUsedByTypeIlScanner,
        IEntityUsedInTypeFinder<T> usageFinder)
    {
        _usageFinder = usageFinder;
        _typeUsedByTypeIlScanner = typeUsedByTypeIlScanner;
        _decompilerFactory = decompilerFactory;
    }
        
    public IEnumerable<DecompileInfo> Run(T symbol)
    {
        var result = new List<DecompileInfo>();

        var typesThatUseType = _typeUsedByTypeIlScanner.GetRootTypesThatUseSymbol(symbol);
            
        var decompiledTypeDefinitions = new ConcurrentDictionary<string, (SyntaxTree, string)>();
        Parallel.ForEach(typesThatUseType, new ParallelOptions {MaxDegreeOfParallelism = 25} , typeToDecompile =>
        {
            var decompiledTypeDefinition = DecompileTypeDefinition(typeToDecompile);
            decompiledTypeDefinitions.TryAdd(typeToDecompile.ReflectionName, decompiledTypeDefinition);
        });

        foreach (var typeToSearch in typesThatUseType)
        {
            if (decompiledTypeDefinitions.TryGetValue(
                    typeToSearch.ReflectionName,
                    out (SyntaxTree syntaxTree, string sourceText) decompiledParentType))
            {
                var usages = _usageFinder.Find(
                    decompiledParentType.syntaxTree,
                    typeToSearch,
                    symbol);
                    
                MapToSourceInfos(typeToSearch, decompiledParentType.sourceText, usages, result);
            }
        }
                
        return result;
    }

    protected (SyntaxTree, string) DecompileTypeDefinition(ITypeDefinition typeDefinition)
    {
        var fileName = typeDefinition.Compilation.MainModule.PEFile.FileName;
        var cachingDecompiler = _decompilerFactory.Get(fileName);

        var (syntaxTree, sourceText) = cachingDecompiler.Run(typeDefinition);
        return (syntaxTree, sourceText);
    }
}