using System.Threading.Tasks;

namespace TryOmnisharpExtension.GotoDefinition
{
    public class DecompileGotoDefinitionHandler : HandlerBase<DecompiledLocationRequest, DecompileGotoDefinitionResponse>
    {
        private readonly RosylnSymbolInfoFinder<IGotoDefinitionCommand> _rosylnGotoDefinitionCommandFactory;
        private readonly IlSpyCommandFactory<IGotoDefinitionCommand> _ilSpySymbolInfoFinder;

        public DecompileGotoDefinitionHandler(
            RosylnSymbolInfoFinder<IGotoDefinitionCommand> rosylnSymbolInfoFinder,
            IlSpyCommandFactory<IGotoDefinitionCommand> ilSpySymbolInfoFinder)
        {
            _rosylnGotoDefinitionCommandFactory = rosylnSymbolInfoFinder;
            _ilSpySymbolInfoFinder = ilSpySymbolInfoFinder;
        }
        
        public override async Task<DecompileGotoDefinitionResponse> Handle(DecompiledLocationRequest request)
        {
            IGotoDefinitionCommand command;
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