using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension
{
    internal class FindTypeImplementationsCommand : IFindImplementationsCommand
    {
        private readonly IlSpyBaseTypeUsageFinder2 _ilSpyTypeFinder;
        private readonly string _projectAssemblyFilePath;
        private readonly ITypeDefinition _typeDefinition;

        public FindTypeImplementationsCommand(
            string projectAssemblyFilePath,
            ITypeDefinition typeDefinition,
            IlSpyBaseTypeUsageFinder2 ilSpyTypeFinder)
        {
            _projectAssemblyFilePath = projectAssemblyFilePath;
            _typeDefinition = typeDefinition;
            _ilSpyTypeFinder = ilSpyTypeFinder;
        }

        public async Task<FindImplementationsResponse> Execute()
        {
            var metadataSources = await _ilSpyTypeFinder.Run(
                _typeDefinition,
                _projectAssemblyFilePath);

            var result = new FindImplementationsResponse();

            foreach (var metadataSource in metadataSources)
            {
                DecompileInfo decompileInfo = DecompileInfoMapper.MapFromMetadataSource(metadataSource);
                result.Implementations.Add(decompileInfo);
            }

            return result;
        }
    }
}