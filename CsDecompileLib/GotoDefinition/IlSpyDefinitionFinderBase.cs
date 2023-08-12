using System;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GotoDefinition;

public class IlSpyDefinitionFinderBase<T> : IlSpyToSourceInfoBase where T : IEntity
{
    private readonly IDefinitionInDecompiledSyntaxTreeFinder<T> _eventInTypeFinder;
    private readonly DecompilerFactory _decompilerFactory;
    private IDefinitionInDecompiledSyntaxTreeFinder<ITypeDefinition> _typeDefinitionFinder;

    public IlSpyDefinitionFinderBase(
        IDefinitionInDecompiledSyntaxTreeFinder<T> eventInTypeFinder,
        IDefinitionInDecompiledSyntaxTreeFinder<ITypeDefinition> typeDefinitionFinder,
        DecompilerFactory decompilerFactory)
    {
        _typeDefinitionFinder = typeDefinitionFinder;
        _eventInTypeFinder = eventInTypeFinder;
        _decompilerFactory = decompilerFactory;
    }
        
    public DecompileInfo Find(T eventSymbol)
    {
        var rootType = SymbolHelper.FindContainingType(eventSymbol);

        var decompiled = DecompileTypeDefinition(rootType);

        var foundUse = _eventInTypeFinder.Find(
            eventSymbol,
            decompiled.syntaxTree);

        //If we can't find the member,  fallback to returning the type definition
        //This most common cause would be a default constructor
        if (foundUse == null)
        {
            foundUse = _typeDefinitionFinder.Find(rootType, decompiled.syntaxTree);
        }
            
        var lines = decompiled.sourceText.Split(new []{"\r\n"}, StringSplitOptions.None);

        var metadataSource = MapToSourceInfo(lines, foundUse, rootType);
        
        return metadataSource;
    }
        
    private (SyntaxTree syntaxTree, string sourceText) DecompileTypeDefinition(ITypeDefinition typeDefinition)
    {
        var fileName = typeDefinition.ParentModule.PEFile.FileName;
        var cachingDecompiler = _decompilerFactory.Get(fileName);

        var (syntaxTree, sourceText) = cachingDecompiler.Run(typeDefinition);
        return (syntaxTree, sourceText);
    }
}