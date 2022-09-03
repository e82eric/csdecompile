using System.Threading.Tasks;
using TryOmnisharpExtension.Roslyn;

namespace TryOmnisharpExtension.GotoDefinition;

public class GetSymbolInfoHandler : HandlerBase<DecompiledLocationRequest, SymbolInfo>
{
    private readonly RoslynLocationToCommandFactory<INavigationCommand<SymbolInfo>> _rosylnGotoDefinitionCommandFactory;
    private readonly IlSpyCommandFactory<INavigationCommand<SymbolInfo>> _ilSpySymbolInfoFinder;

    public GetSymbolInfoHandler(
        RoslynLocationToCommandFactory<INavigationCommand<SymbolInfo>> roslynLocationToCommandFactory,
        IlSpyCommandFactory<INavigationCommand<SymbolInfo>> ilSpySymbolInfoFinder
    )
    {
        _ilSpySymbolInfoFinder = ilSpySymbolInfoFinder;
        _rosylnGotoDefinitionCommandFactory = roslynLocationToCommandFactory;
    }
        
    public override async Task<ResponsePacket<SymbolInfo>> Handle(DecompiledLocationRequest request)
    {
        INavigationCommand<SymbolInfo> command = null;
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