using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharp.Decompiler.IlSpy2;
using TryOmnisharpExtension;
using TryOmnisharpExtension.FindUsages;
using TryOmnisharpExtension.IlSpy;

namespace IlSpy.Analyzer.Extraction
{
    [Export]
    public class IlSpyUsagesFinder
    {
        private readonly TypeUsedByAnalyzer2 _typeUsedByAnalyzer;
        private readonly TypeUsedInTypeFinder _typeUsedInTypeFinder;
        private readonly TypeInMethodDefinitionFinder _typeInMethodDefinitionFinder;
        private readonly TypeInMethodBodyFinder _typeInMethodBodyFinder;

        [ImportingConstructor]
        public IlSpyUsagesFinder(
            TypeUsedByAnalyzer2 typeUsedByAnalyzer,
            TypeUsedInTypeFinder typeUsedInTypeFinder,
            TypeInMethodDefinitionFinder typeInMethodDefinitionFinder,
            TypeInMethodBodyFinder typeInMethodBodyFinder)
        {
            _typeUsedByAnalyzer = typeUsedByAnalyzer;
            _typeUsedInTypeFinder = typeUsedInTypeFinder;
            _typeInMethodDefinitionFinder = typeInMethodDefinitionFinder;
            _typeInMethodBodyFinder = typeInMethodBodyFinder;
        }
        
        public async Task<IEnumerable<IlSpyMetadataSource2>> Run(IEntity symbol)
        {
            var result = new List<IlSpyMetadataSource2>();

            try
            {
                var typeUsedByResult = await _typeUsedByAnalyzer.Analyze(symbol);

                var types = typeUsedByResult.Where(r => r.SymbolKind == SymbolKind.TypeDefinition);

                foreach (var type in types)
                {
                    await AddTypeDefinitionToResult((ITypeDefinition)type, result, symbol);
                }

                var methods = typeUsedByResult.Where(r => r.SymbolKind == SymbolKind.Method);

                foreach (var method in methods)
                {
                    AddMethodDefinitionToResult((IMethod)method, result, symbol);
                }

                var stuffThatNeedsToBeSearched = methods.Where(m =>
                    ((IMethod)m).ReturnType.ReflectionName != symbol.ReflectionName &&
                    !((IMethod)m).Parameters.Any(p => p.Type.ReflectionName == symbol.ReflectionName));

                foreach (var methodToSearch in stuffThatNeedsToBeSearched)
                {
                    var method = methodToSearch as IMethod;
                    var rootType = SymbolHelper.FindContainingType(method);
                    if (method != null)
                    {
                        var foundUses = await _typeInMethodBodyFinder.Find(method, ((ITypeDefinition)symbol).MetadataToken, rootType);

                        foreach (var foundUse in foundUses)
                        {
                            var parentType = SymbolHelper.FindContainingType(method);
                            if (parentType != null)
                            {
                                var metadataSource = new IlSpyMetadataSource2
                                {
                                    AssemblyName = parentType.ParentModule.AssemblyName,
                                    Column = foundUse.StartLocation.Column,
                                    Line = foundUse.StartLocation.Line,
                                    SourceText = $"{method.ReflectionName} {foundUse.Statement.ToString().Replace("\r\n", "")}",
                                    StartColumn = foundUse.StartLocation.Column,
                                    EndColumn = foundUse.EndLocation.Column,
                                    ContainingTypeFullName = parentType.ReflectionName,
                                    AssemblyFilePath = method.Compilation.MainModule.PEFile.FileName,
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

        private async Task AddTypeDefinitionToResult(ITypeDefinition symbol, IList<IlSpyMetadataSource2> result, IEntity typeToSearchFor)
        {
            var parentType = SymbolHelper.FindContainingType(symbol);

            var usagesInTypeDefintions = await _typeUsedInTypeFinder.Find2(parentType, typeToSearchFor.MetadataToken);

            foreach (var usage in usagesInTypeDefintions)
            {
                var metadataSource = new IlSpyMetadataSource2
                {
                    AssemblyName = symbol.ParentModule.AssemblyName,
                    Column = usage.StartLocation.Column,
                    Line = usage.StartLocation.Line,
                    SourceText = symbol.ReflectionName,
                    StartColumn = usage.StartLocation.Column,
                    EndColumn = usage.EndLocation.Column,
                    ContainingTypeFullName = parentType.ReflectionName,
                    AssemblyFilePath = symbol.Compilation.MainModule.PEFile.FileName,
                    UsageType = UsageTypes.InMethodBody
                };

                result.Add(metadataSource);
            }
        }

        private async Task AddMethodDefinitionToResult(IMethod symbol, IList<IlSpyMetadataSource2> result, IEntity typeToSearchFor)
        {
            var parentType = SymbolHelper.FindContainingType(symbol);

            if (parentType != null)
            {
                var rr = await _typeInMethodDefinitionFinder.Find(parentType, symbol, typeToSearchFor.MetadataToken);

                foreach (var r in rr)
                {
                    var metadataSource = new IlSpyMetadataSource2()
                    {
                        AssemblyName = symbol.ParentModule.AssemblyName,
                        Column = r.StartLocation.Column,
                        Line = r.StartLocation.Line,
                        SourceText = symbol.ReflectionName,
                        StartColumn = r.StartLocation.Column,
                        EndColumn = r.EndLocation.Column,
                        ContainingTypeFullName = parentType.ReflectionName,
                        AssemblyFilePath = symbol.Compilation.MainModule.PEFile.FileName,
                        UsageType = UsageTypes.InMethodBody
                    };

                    result.Add(metadataSource);
                }
            }
        }
    }
}
