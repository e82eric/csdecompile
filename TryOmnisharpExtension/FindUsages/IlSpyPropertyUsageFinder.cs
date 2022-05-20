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
    public class IlSpyPropertyUsagesFinder
    {
        private readonly PropertyUsedByAnalyzer _propertyUsedByAnalyzer;
        private readonly PropertyInMethodBodyFinder _propertyInMethodBodyFinder;

        [ImportingConstructor]
        public IlSpyPropertyUsagesFinder(
            PropertyUsedByAnalyzer propertyUsedByAnalyzer,
            PropertyInMethodBodyFinder propertyInMethodBodyFinder)
        {
            _propertyUsedByAnalyzer = propertyUsedByAnalyzer;
            _propertyInMethodBodyFinder = propertyInMethodBodyFinder;
        }
        
        public async Task<IEnumerable<IlSpyMetadataSource2>> Run(IProperty property)
        {
            var result = new List<IlSpyMetadataSource2>();

            try
            {
                var typeUsedByResult = await _propertyUsedByAnalyzer.Analyze(property);

                foreach (var usageToSearch in typeUsedByResult)
                {
                    var method = usageToSearch as IMethod;
                    if (method != null)
                    {
                        var rootType = SymbolHelper.FindContainingType(method);
                        var foundUses = await _propertyInMethodBodyFinder.Find(method, property.Setter?.MetadataToken, property.Getter?.MetadataToken, rootType);

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