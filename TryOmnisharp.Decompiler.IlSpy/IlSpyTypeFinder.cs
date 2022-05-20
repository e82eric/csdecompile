using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.ILSpy;
using ICSharpCode.ILSpy.Analyzers;

namespace IlSpy.Analyzer.Extraction
{
    public class IlSpyTypeFinder
    {
        public (IlSpyMetadataSource, string source) FindDefinitionFromSymbolName(string assemblyFilePath, string symbolFullName)
        {
            try
            {
                var typeSystem = GetTypeSystem(assemblyFilePath);

                var typeToFindDefinitionFor = typeSystem.FindType(new FullTypeName(symbolFullName)) as ITypeDefinition;
                var rootTypeOfTypeToFindDefinitionFor = SymbolHelper.FindContainingType(typeToFindDefinitionFor);

                return FindTypeDefinition(typeToFindDefinitionFor, rootTypeOfTypeToFindDefinitionFor);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static DecompilerTypeSystem GetTypeSystem(string assemblyFilePath)
        {
            var typeSystemFactory = new IlSpyTypeSystemCache();
            //var analyzerContext = new AnalyzerContext();
            var assemblyList = new AssemblyList(new List<string> { assemblyFilePath });
            //analyzerContext.AssemblyList = assemblyList;
            var findAssembly = assemblyList.FindAssembly(assemblyFilePath);
            var result = typeSystemFactory.GetOrCreateTypeSystem(findAssembly.GetPEFileOrNull());
            return result;
        }
        
        private static (IlSpyMetadataSource, string source) FindTypeDefinition(
            ITypeDefinition typeToFindDefinitionFor,
            ITypeDefinition rootTypeOfTypeToFindDefinitionFor)
        {
            var finder = new TypenFinder();
            var (locationOfDefinition, sourceOfDefinition) = finder.Find(typeToFindDefinitionFor, typeToFindDefinitionFor.MetadataToken, rootTypeOfTypeToFindDefinitionFor);

            var fileName = GetFilePathForExternalSymbol(rootTypeOfTypeToFindDefinitionFor);

            var metadataSource = new IlSpyMetadataSource()
            {
                AssemblyName = rootTypeOfTypeToFindDefinitionFor.ParentModule.AssemblyName,
                FileName = fileName,
                Column = locationOfDefinition.StartLocation.Column,
                Line = locationOfDefinition.StartLocation.Line,
                MemberName = typeToFindDefinitionFor.ReflectionName,
                SourceLine = locationOfDefinition.Statement.Replace("\r\n", ""),
                SourceText = $"{typeToFindDefinitionFor.ReflectionName} {locationOfDefinition.Statement.ToString().Replace("\r\n", "")}",
                TypeName = typeToFindDefinitionFor.ReflectionName,
                StatementLine = locationOfDefinition.StartLocation.Line,
                StartColumn = locationOfDefinition.StartLocation.Column,
                EndColumn = locationOfDefinition.EndLocation.Column,
                ContainingTypeFullName = rootTypeOfTypeToFindDefinitionFor.ReflectionName,
                AssemblyFilePath = typeToFindDefinitionFor.Compilation.MainModule.PEFile.FileName
            };

            return (metadataSource, sourceOfDefinition);
        }

        private static string GetFilePathForExternalSymbol(ITypeDefinition topLevelSymbol)
        {
            return $"{topLevelSymbol.Name}.cs";
        }
    }
}
