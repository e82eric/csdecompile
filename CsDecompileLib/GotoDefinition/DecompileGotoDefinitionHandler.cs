using System;
using System.Threading.Tasks;
using CsDecompileLib.Roslyn;

namespace CsDecompileLib.GotoDefinition
{
    public class DecompileGotoDefinitionHandler : HandlerBase<DecompiledLocationRequest, DecompileGotoDefinitionResponse>
    {
        private readonly RoslynLocationToCommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>> _rosylnGotoDefinitionCommandFactory;
        private readonly IlSpyCommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>> _classLevelCommandFactory;
        private readonly IlSpyCommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>> _assemblyLevelCommandFactory;

        public DecompileGotoDefinitionHandler(
            RoslynLocationToCommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>> roslynLocationToCommandFactory,
            IlSpyCommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>> classLevelCommandFactory,
            IlSpyCommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>> assemblyLevelCommandFactory)
        {
            _rosylnGotoDefinitionCommandFactory = roslynLocationToCommandFactory;
            _classLevelCommandFactory = classLevelCommandFactory;
            _assemblyLevelCommandFactory = assemblyLevelCommandFactory;
        }
        
        public override async Task<ResponsePacket<DecompileGotoDefinitionResponse>> Handle(DecompiledLocationRequest request)
        {
            INavigationCommand<DecompileGotoDefinitionResponse> command;

            switch (request.Type)
            {
                case LocationType.SourceCode:
                    command = await _rosylnGotoDefinitionCommandFactory.Get(request);
                    break;
                case LocationType.Decompiled:
                    command = _classLevelCommandFactory.Find(request);
                    break;
                case LocationType.DecompiledAssembly:
                    command = _assemblyLevelCommandFactory.Find(request);
                    break;
                default:
                    throw new Exception("Unknown Location Type");
            }
            
            var result = await command.Execute();
            return result;
        }
    }
}
