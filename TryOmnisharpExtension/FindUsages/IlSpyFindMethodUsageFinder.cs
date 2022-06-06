using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension;
using TryOmnisharpExtension.FindUsages;
using TryOmnisharpExtension.IlSpy;

namespace IlSpy.Analyzer.Extraction
{
    [Export]
    public class IlSpyMethodUsagesFinder
    {
        private readonly MethodUsedByAnalyzer _methodUsedByAnalyzer;
        private readonly MethodInMethodBodyFinder _methodInMethodBodyFinder;

        [ImportingConstructor]
        public IlSpyMethodUsagesFinder(
            MethodUsedByAnalyzer methodUsedByAnalyzer,
            MethodInMethodBodyFinder methodInMethodBodyFinder)
        {
            _methodUsedByAnalyzer = methodUsedByAnalyzer;
            _methodInMethodBodyFinder = methodInMethodBodyFinder;
        }
        public async Task<IEnumerable<IlSpyMetadataSource2>> Run(IMethod methodToFindUsesOf)
        {
            var result = new List<IlSpyMetadataSource2>();

            var usedByResult = await _methodUsedByAnalyzer.Analyze(methodToFindUsesOf);

            foreach (var methodToSearch in usedByResult)
            {
                if (methodToSearch is IMethod methodSymbolToSearch)
                {
                    var rootType = SymbolHelper.FindContainingType(methodSymbolToSearch);
                    var foundUses = await _methodInMethodBodyFinder.Find(
                        methodSymbolToSearch,
                        methodToFindUsesOf.MetadataToken,
                        rootType);

                    foreach (var foundUse in foundUses)
                    {
                        var metadataSource = new IlSpyMetadataSource2
                        {
                            AssemblyName = rootType.ParentModule.AssemblyName,
                            Column = foundUse.StartLocation.Column,
                            Line = foundUse.StartLocation.Line,
                            SourceText =
                                $"{methodSymbolToSearch.ReflectionName} {foundUse.Statement.Replace("\r\n", "")}",
                            StartColumn = foundUse.StartLocation.Column,
                            EndColumn = foundUse.EndLocation.Column,
                            ContainingTypeFullName = rootType.ReflectionName,
                            AssemblyFilePath = methodSymbolToSearch.Compilation.MainModule.PEFile.FileName,
                            UsageType = UsageTypes.InMethodBody
                        };

                        result.Add(metadataSource);
                    }
                }
            }

            return result;
        }
    }
}
