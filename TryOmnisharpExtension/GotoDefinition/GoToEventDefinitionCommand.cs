using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension
{
    public class GotoEventDefintionCommand : IGotoDefinitionCommand
    {
        private readonly IEvent _eventSymbol;
        private readonly IlSpyEventFinder _ilSpyFinder;
        private readonly string _assemblyFilePath;

        public GotoEventDefintionCommand(
            IEvent eventSymbol,
            IlSpyEventFinder ilSpyFinder,
            string assemblyFilePath)
        {
            _eventSymbol = eventSymbol;
            _ilSpyFinder = ilSpyFinder;
            _assemblyFilePath = assemblyFilePath;
        }
        
        public Task<DecompileGotoDefinitionResponse> Execute()
        {
            var (ilSpyMetadataSource, sourceText) = _ilSpyFinder.Find(
                _eventSymbol);
            
            var decompileInfo = DecompileInfoMapper.MapFromMetadataSource(ilSpyMetadataSource);
            decompileInfo.AssemblyFilePath = _assemblyFilePath;
            
            var result = new DecompileGotoDefinitionResponse { Location = decompileInfo, SourceText = sourceText, IsDecompiled = true };

            return Task.FromResult(result);
        }
    }
}