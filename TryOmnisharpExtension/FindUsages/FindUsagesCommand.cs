using System.ComponentModel.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using IlSpy.Analyzer.Extraction;
using TryOmnisharpExtension.FindUsages;

namespace TryOmnisharpExtension
{
    [Export(typeof(INavigationCommand<FindImplementationsResponse>))]
    internal class FindUsagesCommand : INavigationCommand<FindUsagesResponse>
    {
        private readonly ITypeDefinition _typeDefinition;
        private readonly IlSpyUsagesFinder _ilSpyUsagesFinder;
        
        [ImportingConstructor]
        public FindUsagesCommand(
            string projectAssemblyFilePath,
            ITypeDefinition typeDefinition,
            IlSpyUsagesFinder ilSpyUsagesFinder)
        {
            _typeDefinition = typeDefinition;
            _ilSpyUsagesFinder = ilSpyUsagesFinder;
        }
        
        public async Task<FindUsagesResponse> Execute()
        {
            var metadataSources = await _ilSpyUsagesFinder.Run(
                _typeDefinition);

            var result = new FindUsagesResponse();
            
            foreach (var metadataSource in metadataSources)
            {
                DecompileInfo decompileInfo = DecompileInfoMapper.MapFromMetadataSource(metadataSource);
                result.Implementations.Add(decompileInfo);
            }
            
            return result;
        }
    }
}
