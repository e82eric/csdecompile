using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension
{
    class GotoPropertyDefintionCommand : IGotoDefinitionCommand
    {
        private readonly IProperty _property;
        private readonly IlSpyPropertyFinder _ilSpyPropertyFinder;
        private readonly string _assemblyFilePath;

        public GotoPropertyDefintionCommand(
            IProperty property,
            IlSpyPropertyFinder ilSpyPropertyFinder,
            string assemblyFilePath)
        {
            _property = property;
            _ilSpyPropertyFinder = ilSpyPropertyFinder;
            _assemblyFilePath = assemblyFilePath;
        }

        public async Task<DecompileGotoDefinitionResponse> Execute()
        {
            var (ilSpyMetadataSource, sourceText) = await _ilSpyPropertyFinder.Run(
                _property);
            
            var decompileInfo = DecompileInfoMapper.MapFromMetadataSource(ilSpyMetadataSource);
            decompileInfo.AssemblyFilePath = _assemblyFilePath;
            
            var result = new DecompileGotoDefinitionResponse { Location = decompileInfo, SourceText = sourceText, IsDecompiled = true };

            return result;
        }
    }
}