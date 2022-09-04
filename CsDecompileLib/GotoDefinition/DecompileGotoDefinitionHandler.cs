﻿using System.Threading.Tasks;
using CsDecompileLib.Roslyn;

namespace CsDecompileLib.GotoDefinition
{
    public class DecompileGotoDefinitionHandler : HandlerBase<DecompiledLocationRequest, DecompileGotoDefinitionResponse>
    {
        private readonly RoslynLocationToCommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>> _rosylnGotoDefinitionCommandFactory;
        private readonly IlSpyCommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>> _ilSpySymbolInfoFinder;

        public DecompileGotoDefinitionHandler(
            RoslynLocationToCommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>> roslynLocationToCommandFactory,
            IlSpyCommandFactory<INavigationCommand<DecompileGotoDefinitionResponse>> ilSpySymbolInfoFinder)
        {
            _rosylnGotoDefinitionCommandFactory = roslynLocationToCommandFactory;
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