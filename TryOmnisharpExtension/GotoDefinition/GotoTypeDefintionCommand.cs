using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension
{
    public class GotoTypeDefintionCommand : IGotoDefinitionCommand
    {
        private readonly ITypeDefinition _typeDefinition;
        private readonly IlSpyTypeFinder2 _ilSpyTypeFinder;
        private readonly string _assemblyFilePath;

        public GotoTypeDefintionCommand(
            ITypeDefinition typeDefinition,
            IlSpyTypeFinder2 ilSpyTypeFinder,
            string assemblyFilePath)
        {
            _typeDefinition = typeDefinition;
            _ilSpyTypeFinder = ilSpyTypeFinder;
            _assemblyFilePath = assemblyFilePath;
        }
        
        public async Task<DecompileGotoDefinitionResponse> Execute()
        {
            var (ilSpyMetadataSource, sourceText) = await _ilSpyTypeFinder.FindDefinitionFromSymbolName(
                _typeDefinition);
            
            var decompileInfo = DecompileInfoMapper.MapFromMetadataSource(ilSpyMetadataSource);
            decompileInfo.AssemblyFilePath = _assemblyFilePath;
            
            var result = new DecompileGotoDefinitionResponse { Location = decompileInfo, SourceText = sourceText, IsDecompiled = true };

            return result;
        }
    }
}