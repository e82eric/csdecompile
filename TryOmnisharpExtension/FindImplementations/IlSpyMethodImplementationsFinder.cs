﻿using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class IlSpyMethodImplementationFinder
{
    private readonly MethodImplementedByAnalyzer _methodImplementedByAnalyzer;

    [ImportingConstructor]
    public IlSpyMethodImplementationFinder(
        MethodImplementedByAnalyzer methodImplementedByAnalyzer)
    {
        _methodImplementedByAnalyzer = methodImplementedByAnalyzer;
    }
    
    public async Task<IEnumerable<IlSpyMetadataSource2>> Run(IMethod method, string projectName)
    {
        var result = new List<IlSpyMetadataSource2>();

        var implementations = await _methodImplementedByAnalyzer.Analyze(method);

        foreach (var implementation in implementations)
        {
            await AddToResult((ITypeDefinition)implementation.DeclaringType, implementation, method.Name, projectName, result);
        }

        return result;
    }

    private async Task AddToResult(
        ITypeDefinition symbol,
        IMethod method,
        string methodName,
        string projectName,
        IList<IlSpyMetadataSource2> result)
    {
        var parentType = SymbolHelper.FindContainingType(symbol);
        var assemblyFullName = symbol.ParentModule.PEFile.FullName;
        var dotNetVersion = symbol.ParentModule.PEFile.Metadata.MetadataVersion;
        var sourceText = $"{method.FullName} ({assemblyFullName}, .net: {dotNetVersion})";
        
        var assemblyVersion = symbol.ParentModule.PEFile.Metadata.GetAssemblyDefinition().Version.ToString();
        var typeFullName = GetTypeFullName(method);

        if (symbol.ParentModule.AssemblyName != projectName)
        {
            var metadataSource = new IlSpyMetadataSource2
            {
                AssemblyName = symbol.ParentModule.AssemblyName,
                SourceText = sourceText,
                ContainingTypeFullName = parentType.ReflectionName,
                AssemblyFilePath = symbol.Compilation.MainModule.PEFile.FileName,
                UsageType = UsageTypes.MethodImplementation,
                NamespaceName = symbol.Namespace,
                TypeName = symbol.Name,
                MethodName = methodName,
                DotNetVersion = dotNetVersion,
                AssemblyVersion = assemblyVersion,
                TypeFullName = typeFullName
            };
            result.Add(metadataSource);
        }
    }
    
    private string GetTypeFullName(IMethod method)
    {
        var methodSignaurePart = GetMethodSignatureDisplayString(method);
        var result = $"{method.DeclaringType.Name}.{method.Name}{methodSignaurePart}";
        var ittr = method.DeclaringType;
        
        while (ittr.DeclaringType != null)
        {
            result = $"{ittr.Name}+{result}";
            ittr = (ITypeDefinition)ittr.DeclaringType;
        }

        return result;
    }

    private string GetMethodSignatureDisplayString(IMethod method)
    {
        var paramTypes = new List<string>();
        foreach (var methodParameter in method.Parameters)
        {
            paramTypes.Add(methodParameter.Type.Name);
        }

        var result = $"({string.Join(",", paramTypes)})";
        return result;
    }
}