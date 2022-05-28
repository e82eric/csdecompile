using System.Composition;
using System.Threading.Tasks;
using OmniSharp.Mef;

namespace TryOmnisharpExtension
{
    [OmniSharpHandler(Endpoints.DecompileGotoDefinition, Languages.Csharp), Shared]
    public class DecompileGotoDefinitionHandler : IRequestHandler<DecompileGotoDefinitionRequest, DecompileGotoDefinitionResponse>
    {
        private readonly RosylnSymbolInfoFinder<IGotoDefinitionCommand> _rosylnGotoDefinitionCommandFactory;
        private readonly IlSpyCommandFactory<IGotoDefinitionCommand> _ilSpySymbolInfoFinder;

        [ImportingConstructor]
        public DecompileGotoDefinitionHandler(
            RosylnSymbolInfoFinder<IGotoDefinitionCommand> rosylnSymbolInfoFinder,
            IlSpyCommandFactory<IGotoDefinitionCommand> ilSpySymbolInfoFinder)
        {
            _rosylnGotoDefinitionCommandFactory = rosylnSymbolInfoFinder;
            _ilSpySymbolInfoFinder = ilSpySymbolInfoFinder;
        }
        
        public async Task<DecompileGotoDefinitionResponse> Handle(DecompileGotoDefinitionRequest request)
        {
            IGotoDefinitionCommand command;
            if (!request.IsDecompiled)
            {
                command = await _rosylnGotoDefinitionCommandFactory.Get(request);
            }
            else
            {
                command = await _ilSpySymbolInfoFinder.Find(request);
            }
            
            var result = await command.Execute();
            return result;
        }
    }
}