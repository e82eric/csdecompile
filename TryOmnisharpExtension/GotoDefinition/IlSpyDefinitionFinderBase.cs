using System;
using System.Composition;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GotoDefinition;

public class IlSpyDefinitionFinderBase<T> : IlSpyToSourceInfoBase where T : IEntity
{
    private readonly IDefinitionInDecompiledSyntaxTreeFinder<T> _eventInTypeFinder;
    private readonly DecompilerFactory _decompilerFactory;

    [ImportingConstructor]
    public IlSpyDefinitionFinderBase(IDefinitionInDecompiledSyntaxTreeFinder<T> eventInTypeFinder, DecompilerFactory decompilerFactory)
    {
        _eventInTypeFinder = eventInTypeFinder;
        _decompilerFactory = decompilerFactory;
    }
        
    public (DecompileInfo, string) Find(T eventSymbol)
    {
        var rootType = SymbolHelper.FindContainingType(eventSymbol);

        var decompiled = DecompileTypeDefinition(rootType);

        var foundUse = _eventInTypeFinder.Find(
            eventSymbol,
            decompiled.syntaxTree);
            
        var lines = decompiled.sourceText.Split(new []{"\r\n"}, StringSplitOptions.None);

        var metadataSource = MapToSourceInfo(lines, foundUse, rootType);
        
        return (metadataSource, decompiled.sourceText);
    }
        
    private (SyntaxTree syntaxTree, string sourceText) DecompileTypeDefinition(ITypeDefinition typeDefinition)
    {
        var fileName = typeDefinition.ParentModule.PEFile.FileName;
        var cachingDecompiler = _decompilerFactory.Get(fileName);

        var (syntaxTree, sourceText) = cachingDecompiler.Run(typeDefinition);
        return (syntaxTree, sourceText);
    }
}