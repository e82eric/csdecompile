using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.ILSpy;
using ICSharpCode.ILSpy.Analyzers;
using TryOmnisharp.Decompiler.IlSpy;

namespace IlSpy.Analyzer.Extraction
{
    public class IlSpyBaseTypeUsageFinder
    {
        public IEnumerable<IlSpyMetadataSource> Run(List<string> assemblyPaths, string typeName)
        {
            var result = new List<IlSpyMetadataSource>();

            try
            {
                var typeSystemFactory = new IlSpyTypeSystemCache();
                //var cont1 = new AnalyzerContext();
                var assemblyList = new AssemblyList(assemblyPaths);
                //cont1.AssemblyList = assemblyList;
                var findAssembly = assemblyList.FindAssembly(assemblyPaths.First());
                var ts = typeSystemFactory.GetOrCreateTypeSystem(findAssembly.GetPEFileOrNull());

                var findType = ts.FindType(new FullTypeName(typeName));

                var typeUsedByAnalyzer = new TypeUsedByAnalyzer();

                var typeUsedByResult = typeUsedByAnalyzer.Analyze((ITypeDefinition)findType, true, assemblyList).ToList();

                var types = typeUsedByResult.Where(r => r.SymbolKind == SymbolKind.TypeDefinition);

                var typeGroups = types.GroupBy(m =>
                {
                    var parentType = FindContainingType((ITypeDefinition)m);
                    return parentType.ReflectionName;
                });

                foreach (var type in types)
                {
                    AddTypeDefinitionToResult((ITypeDefinition)type, result, (ITypeDefinition)findType);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return result;
        }

        private static void AddTypeDefinitionToResult(ITypeDefinition symbol, IList<IlSpyMetadataSource> result, IEntity typeToSearchFor)
        {
            var parentType = FindContainingType(symbol);

            var finder = new TypeUsedInTypeFinder();
            var usagesInTypeDefintions = finder.Find2(parentType, typeToSearchFor.MetadataToken);

            foreach (var usage in usagesInTypeDefintions)
            {
                var fileName = GetFilePathForExternalSymbol(parentType);
                var metadataSource = new IlSpyMetadataSource
                {
                    AssemblyName = symbol.ParentModule.AssemblyName,
                    FileName = fileName,
                    Column = usage.StartLocation.Column,
                    Line = usage.StartLocation.Line,
                    MemberName = symbol.ReflectionName,
                    SourceLine = symbol.FullName,
                    SourceText = symbol.ReflectionName,
                    TypeName = symbol.FullName,
                    StartColumn = usage.StartLocation.Column,
                    EndColumn = usage.EndLocation.Column,
                    ContainingTypeFullName = parentType.ReflectionName,
                    AssemblyFilePath = symbol.Compilation.MainModule.PEFile.FileName
                };

                result.Add(metadataSource);
            }
        }

        private static ITypeDefinition FindContainingType(ITypeDefinition symblol)
        {
            IType result = symblol;

            while (result.DeclaringType != null)
            {
                result = result.DeclaringType;
            }

            if (result is ParameterizedType)
            {
                return null;
            }
            return (ITypeDefinition)result;
        }

        private static string GetFilePathForExternalSymbol(ITypeDefinition topLevelSymbol)
        {
            return $"{topLevelSymbol.Name}.cs";
        }
    }
}