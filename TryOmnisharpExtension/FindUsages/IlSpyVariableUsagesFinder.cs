using System.Collections.Generic;
using System.Composition;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages;

[Export]
public class IlSpyVariableUsagesFinder
{
    private readonly VariableInMethodBodyFinder _variableInMethodBodyFinder;

    [ImportingConstructor]
    public IlSpyVariableUsagesFinder(
        VariableInMethodBodyFinder variableInMethodBodyFinder)
    {
        _variableInMethodBodyFinder = variableInMethodBodyFinder;
    }
        
    public IEnumerable<IlSpyMetadataSource2> Run(ITypeDefinition containingTypeDefinition, AstNode variable)
    {
        var result = new List<IlSpyMetadataSource2>();

        var foundUses = _variableInMethodBodyFinder.Find((Identifier)variable);

        foreach (var foundUse in foundUses)
        {
            var metadataSource = new IlSpyMetadataSource2
            {
                AssemblyName = containingTypeDefinition.ParentModule.AssemblyName,
                Column = foundUse.StartLocation.Column,
                Line = foundUse.StartLocation.Line,
                SourceText = foundUse.Statement.Replace("\r\n", ""),
                StartColumn = foundUse.StartLocation.Column,
                EndColumn = foundUse.EndLocation.Column,
                ContainingTypeFullName = containingTypeDefinition.ReflectionName,
                AssemblyFilePath = containingTypeDefinition.ParentModule.PEFile.FileName,
                UsageType = UsageTypes.InMethodBody,
                TypeName = containingTypeDefinition.Name
            };

            result.Add(metadataSource);
        }

        return result;
    }
}