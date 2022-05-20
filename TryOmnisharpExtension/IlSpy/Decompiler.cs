using System;
using System.IO;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;

public class Decompiler
{
    private readonly CSharpDecompiler _decompiler;
    private readonly DecompilerSettings _decompilerSettings;
    private readonly DecompilerTypeSystem _decompilerTypeSystem;

    public Decompiler(DecompilerTypeSystem decompilerTypeSystem)
    {
        _decompilerTypeSystem = decompilerTypeSystem;
        _decompilerSettings = new DecompilerSettings();
        _decompiler = new CSharpDecompiler(decompilerTypeSystem, _decompilerSettings);
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

    public (SyntaxTree, string) Run(string rootTypeFullName)
    {
        var rootType = _decompilerTypeSystem.FindType(new FullTypeName(rootTypeFullName)) as ITypeDefinition;
        var result = Run(rootType);
        return result;
    }
}
