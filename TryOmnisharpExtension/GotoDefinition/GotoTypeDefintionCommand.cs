using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension
{
    public class GotoTypeDefintionCommand : IGotoDefinitionCommand
    {
        private readonly ITypeDefinition _typeDefinition;
        private readonly IlSpyTypeFinder _ilSpyTypeFinder;
        private readonly string _assemblyFilePath;

        public GotoTypeDefintionCommand(
            ITypeDefinition typeDefinition,
            IlSpyTypeFinder ilSpyTypeFinder,
            string assemblyFilePath)
        {
            _typeDefinition = typeDefinition;
            _ilSpyTypeFinder = ilSpyTypeFinder;
            _assemblyFilePath = assemblyFilePath;
        }
        
        public Task<DecompileGotoDefinitionResponse> Execute()
        {
            var (ilSpyMetadataSource, sourceText) = _ilSpyTypeFinder.FindDefinitionFromSymbolName(
                _typeDefinition);
            
            var decompileInfo = DecompileInfoMapper.MapFromMetadataSource(ilSpyMetadataSource);
            decompileInfo.AssemblyFilePath = _assemblyFilePath;
            
            var result = new DecompileGotoDefinitionResponse { Location = decompileInfo, SourceText = sourceText, IsDecompiled = true };

            return Task.FromResult(result);
        }
    }
}