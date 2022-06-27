﻿using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages;

public class IlSpyVariableUsagesFinder : IlSpyToSourceInfoBase
{
    private readonly VariableInMethodBodyFinder _variableInMethodBodyFinder;

    public IlSpyVariableUsagesFinder(
        VariableInMethodBodyFinder variableInMethodBodyFinder)
    {
        _variableInMethodBodyFinder = variableInMethodBodyFinder;
    }
        
    public IEnumerable<DecompileInfo> Run(ITypeDefinition containingTypeDefinition, AstNode variable, string sourceText)
    {
        var result = new List<DecompileInfo>();

        var foundUses = _variableInMethodBodyFinder.Find((Identifier)variable);

        MapToSourceInfos(containingTypeDefinition, sourceText, foundUses, result);

        return result;
    }
}