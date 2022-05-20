using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.ILSpy;
using ICSharpCode.ILSpy.Analyzers;

namespace IlSpy.Analyzer.Extraction
{
    public class IlSpyPropertyFinder
    {
        public (IlSpyMetadataSource, string) Run(string fileName, string typeName, string propertyName)
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

                var property = FindProperty(findType, propertyName);

                var rootType = FindContainingType(findType);

                var finder = new PropertyInTypeFinder();
                var (foundUse, sourceText) = finder.Find(property, property.Setter?.MetadataToken, property.Getter?.MetadataToken, rootType);

                var metadataSource = new IlSpyMetadataSource()
                {
                    AssemblyName = rootType.ParentModule.AssemblyName,
                    FileName = fileName,
                    Column = foundUse.StartLocation.Column,
                    Line = foundUse.StartLocation.Line,
                    MemberName = findType.ReflectionName,
                    SourceLine = foundUse.Statement.Replace("\r\n", ""),
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
        
        public static IProperty FindProperty(IType type, string methodName)
        {
            var properties = type.GetProperties().Where(m =>
            {
                if (m.FullName != methodName)
                {
                    return false;
                }

                return true;
            });

            return (IProperty)properties.FirstOrDefault();
        }

        private static ITypeDefinition FindContainingType(ITypeDefinition symbol)
        {
            if (symbol.DeclaringType == null)
            {
                return symbol;
            }
            IType result = symbol.DeclaringType;

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
    }
}
