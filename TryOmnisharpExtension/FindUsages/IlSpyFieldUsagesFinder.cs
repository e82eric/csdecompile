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
public class IlSpyFieldUsagesFinder
{
    private readonly FieldUsedByAnalyzer _fieldUsedByAnalyzer;
    private readonly FieldInMethodBodyFinder _fieldInMethodBodyFinder;

    [ImportingConstructor]
    public IlSpyFieldUsagesFinder(
        FieldUsedByAnalyzer fieldUsedByAnalyzer,
        FieldInMethodBodyFinder fieldInMethodBodyFinder)
    {
        _fieldUsedByAnalyzer = fieldUsedByAnalyzer;
        _fieldInMethodBodyFinder = fieldInMethodBodyFinder;
    }
        
    public async Task<IEnumerable<IlSpyMetadataSource2>> Run(IField field)
    {
        var result = new List<IlSpyMetadataSource2>();

        try
        {
            var typeUsedByResult = await _fieldUsedByAnalyzer.Analyze(field);

            foreach (var usageToSearch in typeUsedByResult)
            {
                var fieldUsage = usageToSearch as IMember;
                if (fieldUsage != null)
                {
                    var rootType = SymbolHelper.FindContainingType(fieldUsage);
                    var foundUses = await _fieldInMethodBodyFinder.Find(fieldUsage, rootType, field);

                    foreach (var foundUse in foundUses)
                    {
                        var parentType = SymbolHelper.FindContainingType(fieldUsage);
                        if (parentType != null)
                        {
                            var metadataSource = new IlSpyMetadataSource2
                            {
                                AssemblyName = parentType.ParentModule.AssemblyName,
                                Column = foundUse.StartLocation.Column,
                                Line = foundUse.StartLocation.Line,
                                SourceText = $"{fieldUsage.ReflectionName} {foundUse.Statement.ToString().Replace("\r\n", "")}",
                                StartColumn = foundUse.StartLocation.Column,
                                EndColumn = foundUse.EndLocation.Column,
                                ContainingTypeFullName = parentType.ReflectionName,
                                AssemblyFilePath = fieldUsage.Compilation.MainModule.PEFile.FileName,
                                UsageType = UsageTypes.InMethodBody
                            };

                            result.Add(metadataSource);
                        }
                    }
                }
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
                    UsageType = UsageTypes.InMethodBody
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
