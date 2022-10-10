using System.Collections.Generic;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.CSharp.Syntax;
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