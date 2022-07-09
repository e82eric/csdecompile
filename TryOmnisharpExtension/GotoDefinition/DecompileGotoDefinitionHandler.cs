using System.Threading.Tasks;
using TryOmnisharpExtension.Roslyn;

namespace TryOmnisharpExtension.GotoDefinition
{
    public class DecompileGotoDefinitionHandler : HandlerBase<DecompiledLocationRequest, DecompileGotoDefinitionResponse>
    {
        private readonly RosylnSymbolInfoFinder<INavigationCommand<DecompileGotoDefinitionResponse>> _rosylnGotoDefinitionCommandFactory;
        private readonly IlSpyCommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>> _ilSpySymbolInfoFinder;

        public DecompileGotoDefinitionHandler(
            RosylnSymbolInfoFinder<INavigationCommand<DecompileGotoDefinitionResponse>> rosylnSymbolInfoFinder,
            IlSpyCommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>> ilSpySymbolInfoFinder)
        {
            _rosylnGotoDefinitionCommandFactory = rosylnSymbolInfoFinder;
            _ilSpySymbolInfoFinder = ilSpySymbolInfoFinder;
        }
        
        public override async Task<ResponsePacket<DecompileGotoDefinitionResponse>> Handle(DecompiledLocationRequest request)
        {
            INavigationCommand<DecompileGotoDefinitionResponse> command;
            if (!request.IsDecompiled)
            {
                command = await _rosylnGotoDefinitionCommandFactory.Get(request);
            }
            else
            {
                command = _ilSpySymbolInfoFinder.Find(request);
            }
            
            var result = await command.Execute();
            return result;
        }
    }
}
