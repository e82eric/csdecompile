﻿using System.Threading.Tasks;

namespace TryOmnisharpExtension.GotoDefinition
{
    public class DecompileGotoDefinitionHandler
    {
        private readonly RosylnSymbolInfoFinder<IGotoDefinitionCommand> _rosylnGotoDefinitionCommandFactory;
        private readonly IlSpyCommandFactory<IGotoDefinitionCommand> _ilSpySymbolInfoFinder;
        private readonly IlSpyCommandFactory<IGotoDefinitionCommand> _externalAssembliesCommandFactory;

        public DecompileGotoDefinitionHandler(
            RosylnSymbolInfoFinder<IGotoDefinitionCommand> rosylnSymbolInfoFinder,
            IlSpyCommandFactory<IGotoDefinitionCommand> ilSpySymbolInfoFinder,
            IlSpyCommandFactory<IGotoDefinitionCommand> externalAssembliesCommandFactory)
        {
            _rosylnGotoDefinitionCommandFactory = rosylnSymbolInfoFinder;
            _ilSpySymbolInfoFinder = ilSpySymbolInfoFinder;
            _externalAssembliesCommandFactory = externalAssembliesCommandFactory;
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
                if (request.IsFromExternalAssembly)
                {
                    command = _externalAssembliesCommandFactory.Find(request);
                }
                else
                {
                    command = _ilSpySymbolInfoFinder.Find(request);
                }
            }
            
            var result = await command.Execute();
            return result;
        }
    }
}