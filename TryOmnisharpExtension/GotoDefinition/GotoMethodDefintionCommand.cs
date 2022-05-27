using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension
{
    internal class GotoMethodDefintionCommand : IGotoDefinitionCommand
    {
        private readonly IMethod _method;
        private readonly IlSpyMemberFinder _ilSpyMemberFinder;
        private readonly string _projectAssemblyPath;

        public GotoMethodDefintionCommand(
            IMethod method,
            IlSpyMemberFinder ilSpyMemberFinder,
            string projectAssemblyPath)
        {
            _ilSpyMemberFinder = ilSpyMemberFinder;
            _projectAssemblyPath = projectAssemblyPath;
            _method = method;
        }
        
        public async Task<DecompileGotoDefinitionResponse> Execute()
        {
            var (ilSpyMetadataSource, sourceText) = await _ilSpyMemberFinder.Run(_method);
            
            var decompileInfo = DecompileInfoMapper.MapFromMetadataSource(ilSpyMetadataSource);
            decompileInfo.AssemblyFilePath = _projectAssemblyPath;
            
            var result = new DecompileGotoDefinitionResponse
            {
                Location = decompileInfo,
                SourceText = sourceText,
                IsDecompiled = true
            };

            return result;
        }
    }
}