﻿using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;
using SequencePoint = ICSharpCode.Decompiler.DebugInfo.SequencePoint;

namespace CsDecompileLib.IlSpy;

public class Decompiler
{
    private readonly CSharpDecompiler _decompiler;
    private readonly DecompilerSettings _decompilerSettings;
    private readonly DecompilerTypeSystem _decompilerTypeSystem;

    public Decompiler(DecompilerTypeSystem decompilerTypeSystem)
    {
        _decompilerTypeSystem = decompilerTypeSystem;
        _decompilerSettings = new DecompilerSettings
        {
            ShowXmlDocumentation = false
        };
        _decompiler = new CSharpDecompiler(decompilerTypeSystem, _decompilerSettings);
    }

    public Dictionary<ILFunction, List<SequencePoint>> CreateSequencePoints(SyntaxTree syntaxTree)
    {
        var result = _decompiler.CreateSequencePoints(syntaxTree);
        return result;
    }
    
    public (SyntaxTree, string) Run(EntityHandle typeHandle)
    {
        var stringWriter = new StringWriter();
        var tokenWriter = TokenWriter.CreateWriterThatSetsLocationsInAST(stringWriter, "  ");

        var syntaxTree = _decompiler.Decompile(typeHandle);
        syntaxTree.AcceptVisitor(new CSharpOutputVisitor(tokenWriter, _decompilerSettings.CSharpFormattingOptions));

        var source = stringWriter.ToString();

        return (syntaxTree, source);
    }
    
    public (SyntaxTree, string) Run(ITypeDefinition rooTypeDefinition)
    {
        var rootTypeDefinitionHandle = rooTypeDefinition.MetadataToken;

        var stringWriter = new StringWriter();
        var tokenWriter = TokenWriter.CreateWriterThatSetsLocationsInAST(stringWriter, "  ");

        var syntaxTree = _decompiler.DecompileTypes(new[] { (TypeDefinitionHandle)rootTypeDefinitionHandle });
        syntaxTree.AcceptVisitor(new CSharpOutputVisitor(tokenWriter, _decompilerSettings.CSharpFormattingOptions));

        var source = stringWriter.ToString();

        return (syntaxTree, source);
    }

    public SyntaxTree Run(string rootTypeFullName)
    {
        var rootType = _decompilerTypeSystem.FindType(new FullTypeName(rootTypeFullName)) as ITypeDefinition;
        var rootTypeDefinitionHandle = rootType.MetadataToken;

        var stringWriter = new StringWriter();
        var tokenWriter = TokenWriter.CreateWriterThatSetsLocationsInAST(stringWriter, "  ");

        var syntaxTree = _decompiler.DecompileTypes(new[] { (TypeDefinitionHandle)rootTypeDefinitionHandle });
        syntaxTree.AcceptVisitor(new CSharpOutputVisitor(tokenWriter, _decompilerSettings.CSharpFormattingOptions));

        return syntaxTree;
    }

    public (SyntaxTree, string) DecompileWholeModule()
    {
        var stringWriter = new StringWriter();
        var tokenWriter = TokenWriter.CreateWriterThatSetsLocationsInAST(stringWriter, "  ");
        var syntaxTree = _decompiler.DecompileWholeModuleAsSingleFile();
        syntaxTree.AcceptVisitor(new CSharpOutputVisitor(tokenWriter, _decompilerSettings.CSharpFormattingOptions));
        var result = stringWriter.ToString();
        return (syntaxTree, result);
    }

    public IDecompilerTypeSystem TypeSystem
    {
        get { return _decompiler.TypeSystem; }
    }
}
