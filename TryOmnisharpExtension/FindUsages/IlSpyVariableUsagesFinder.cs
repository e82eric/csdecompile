using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension;
using TryOmnisharpExtension.FindUsages;
using TryOmnisharpExtension.IlSpy;

namespace IlSpy.Analyzer.Extraction;

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
        
    public async Task<IEnumerable<IlSpyMetadataSource2>> Run(ITypeDefinition containingTypeDefinition, AstNode variable)
    {
        var result = new List<IlSpyMetadataSource2>();

        try
        {
            var foundUses = await _variableInMethodBodyFinder.Find((Identifier)variable);

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
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return result;
    }
}