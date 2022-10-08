using System;
using System.Collections.Generic;
using CsDecompileLib.GotoDefinition;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.DebugInfo;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindUsages;

public class IlSpyVariableUsagesFinder : IlSpyToSourceInfoBase
{
    private readonly VariableInMethodBodyFinder _variableInMethodBodyFinder;

    public IlSpyVariableUsagesFinder(
        VariableInMethodBodyFinder variableInMethodBodyFinder)
    {
        _variableInMethodBodyFinder = variableInMethodBodyFinder;
    }
        
    public IEnumerable<DecompileInfo> Run(ITypeDefinition containingTypeDefinition, AstNode methodBody, ILVariable variable, string sourceText)
    {
        var result = new List<DecompileInfo>();

        var foundUses = _variableInMethodBodyFinder.Find(methodBody, variable);

        MapToSourceInfos(containingTypeDefinition, sourceText, foundUses, result);

        return result;
    }
}

public class IlSpyVariableDefintionFinder : IlSpyToSourceInfoBase
{
    private readonly VariableInTypeFinder _variableInMethodBodyFinder;

    public IlSpyVariableDefintionFinder(
        VariableInTypeFinder variableInMethodBodyFinder)
    {
        _variableInMethodBodyFinder = variableInMethodBodyFinder;
    }
        
    public DecompileInfo Run(ITypeDefinition containgTypeDefinition, AstNode methodNode, ILVariable variable, string sourceText)
    {
        var foundNode = _variableInMethodBodyFinder.Find(variable, methodNode);

        var lines = sourceText.Split(new []{"\r\n"}, StringSplitOptions.None);
        var result = MapToSourceInfo(lines, foundNode, containgTypeDefinition);

        return result;
    }
}
