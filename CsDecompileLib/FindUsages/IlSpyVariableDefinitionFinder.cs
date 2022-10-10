using System;
using CsDecompileLib.GotoDefinition;
using CsDecompileLib.IlSpy;
using CsDecompileLib.IlSpy.Ast;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindUsages;

public class IlSpyVariableDefinitionFinder : IlSpyToSourceInfoBase
{
    private readonly VariableNodeInTypeAstFinder _variableNodeInMethodBodyAstFinder;

    public IlSpyVariableDefinitionFinder(
        VariableNodeInTypeAstFinder variableNodeInMethodBodyAstFinder)
    {
        _variableNodeInMethodBodyAstFinder = variableNodeInMethodBodyAstFinder;
    }
        
    public DecompileInfo Run(ITypeDefinition containgTypeDefinition, AstNode methodNode, ILVariable variable, string sourceText)
    {
        var foundNode = _variableNodeInMethodBodyAstFinder.Find(variable, methodNode);

        var lines = sourceText.Split(new []{"\r\n"}, StringSplitOptions.None);
        var result = MapToSourceInfo(lines, foundNode, containgTypeDefinition);

        return result;
    }
}