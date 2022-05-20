using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
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
        public async Task<IEnumerable<IlSpyMetadataSource2>> Run(IMethod meth)
        {
            var result = new List<IlSpyMetadataSource2>();

            try
            {
                var typeUsedByResult = await _methodUsedByAnalyzer.Analyze(meth);

                foreach (var methodToSearch in typeUsedByResult)
                {
                    var methodSymbolToSearch = methodToSearch as IMethod;
                    var rootType = SymbolHelper.FindContainingType(methodSymbolToSearch);
                    if (methodSymbolToSearch != null)
                    {
                        var foundUses = await _methodInMethodBodyFinder.Find(
                            methodSymbolToSearch,
                            meth.MetadataToken,
                            rootType);

                        foreach (var foundUse in foundUses)
                        {
                            var parentType = SymbolHelper.FindContainingType(methodSymbolToSearch);
                            if (parentType != null)
                            {
                                var metadataSource = new IlSpyMetadataSource2()
                                {
                                    AssemblyName = parentType.ParentModule.AssemblyName,
                                    Column = foundUse.StartLocation.Column,
                                    Line = foundUse.StartLocation.Line,
                                    SourceText = $"{methodSymbolToSearch.ReflectionName} {foundUse.Statement.Replace("\r\n", "")}",
                                    StartColumn = foundUse.StartLocation.Column,
                                    EndColumn = foundUse.EndLocation.Column,
                                    ContainingTypeFullName = parentType.ReflectionName,
                                    AssemblyFilePath = methodSymbolToSearch.Compilation.MainModule.PEFile.FileName
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
}
