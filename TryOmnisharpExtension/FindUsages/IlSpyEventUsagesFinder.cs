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
    public class IlSpyEventUsagesFinder
    {
        private readonly EventUsedByAnalyzer _propertyUsedByAnalyzer;
        private readonly EventInMethodBodyFinder _eventInMethodBodyFinder;

        [ImportingConstructor]
        public IlSpyEventUsagesFinder(
            EventUsedByAnalyzer propertyUsedByAnalyzer,
            EventInMethodBodyFinder eventInMethodBodyFinder)
        {
            _propertyUsedByAnalyzer = propertyUsedByAnalyzer;
            _eventInMethodBodyFinder = eventInMethodBodyFinder;
        }
        
        public async Task<IEnumerable<IlSpyMetadataSource2>> Run(IEvent eventSymbol)
        {
            var result = new List<IlSpyMetadataSource2>();

            try
            {
                var typeUsedByResult = await _propertyUsedByAnalyzer.Analyze(eventSymbol);

                foreach (var usageToSearch in typeUsedByResult)
                {
                    var method = usageToSearch as IMethod;
                    if (method != null)
                    {
                        var rootType = SymbolHelper.FindContainingType(method);
                        var foundUses = await _eventInMethodBodyFinder.Find(method, eventSymbol.MetadataToken, rootType);

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
                                    AssemblyFilePath = method.Compilation.MainModule.PEFile.FileName
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
