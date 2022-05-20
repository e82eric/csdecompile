using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.ILSpy;
using ICSharpCode.ILSpy.Analyzers;

namespace IlSpy.Analyzer.Extraction;

public class IlSpyMemberFinder
{
    public (IlSpyMetadataSource, string sourceText) Run(string fileName, string typeName, string methodName, IReadOnlyList<string> methodParameterTypes)
    {
        try
        {
            var typeSystemFactory = new IlSpyTypeSystemCache();
            //var analyzerContext = new AnalyzerContext();
            var assemblyList = new AssemblyList(new List<string>() { fileName });
            //analyzerContext.AssemblyList = assemblyList;
            var findAssembly = assemblyList.FindAssembly(fileName);
            var typeSystem = typeSystemFactory.GetOrCreateTypeSystem(findAssembly.GetPEFileOrNull());

            var findType = typeSystem.FindType(new FullTypeName(typeName)) as ITypeDefinition;

            var method = FindMethod(findType, methodName, methodParameterTypes);

            var rootType = SymbolHelper.FindContainingType(findType);

            var finder = new MethodInTypeFinder();
            var (foundUse, sourceText) = finder.Find(findType, method.MetadataToken, rootType);

            var decompiledFileName = GetFilePathForExternalSymbol(rootType);

            var metadataSource = new IlSpyMetadataSource()
            {
                AssemblyName = rootType.ParentModule.AssemblyName,
                FileName = decompiledFileName,
                Column = foundUse.StartLocation.Column,
                Line = foundUse.StartLocation.Line,
                MemberName = findType.ReflectionName,
                SourceLine = foundUse.Statement.ToString().Replace("\r\n", ""),
                SourceText = $"{findType.ReflectionName} {foundUse.Statement.ToString().Replace("\r\n", "")}",
                TypeName = findType.ReflectionName,
                StatementLine = foundUse.StartLocation.Line,
                StartColumn = foundUse.StartLocation.Column,
                EndColumn = foundUse.EndLocation.Column,
                ContainingTypeFullName = rootType.ReflectionName,
                AssemblyFilePath = findType.Compilation.MainModule.PEFile.FileName
            };

            return (metadataSource, sourceText);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private IMember FindMethod(ITypeDefinition type, string methodName, IReadOnlyList<string> methodParameterTypes)
    {
        var methods = new List<IMember>();

        foreach (var member in type.Members)
        {
            if (member.FullName == methodName)
            {
                var method = member as IMethod;

                if (method != null)
                {
                    if (method.Parameters.Count == methodParameterTypes.Count)
                    {
                        var paramsMatch = true;
                        for (int i = 0; i < methodParameterTypes.Count; i++)
                        {
                            if (method.Parameters[i].Type.ReflectionName != methodParameterTypes[i])
                            {
                                paramsMatch = false;
                            }
                        }

                        if (paramsMatch)
                        {
                            methods.Add(member);
                        }
                    }
                }
            }
        }

        if (methods.Count > 1)
        {
            foreach (var method in methods)
            {
                if (method.DeclaringType?.Name == type.Name)
                {
                    return method;
                }
            }
        }

        return methods.FirstOrDefault();
    }

    private static string GetFilePathForExternalSymbol(ITypeDefinition topLevelSymbol)
    {
        return $"{topLevelSymbol.Name}.cs";
    }
}