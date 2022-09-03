using System.Threading.Tasks;

namespace CsDecompileLib.FindImplementations;

public class DecompileFindImplementationsHandler : HandlerBase<DecompiledLocationRequest, FindImplementationsResponse>
{
    private readonly EverywhereSymbolInfoFinder<FindImplementationsResponse> _everywhereSymbolInfoFinder;
    private readonly GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse> _genericIlSpyFindImplementationsCommandFactory;

    public DecompileFindImplementationsHandler(
        EverywhereSymbolInfoFinder<FindImplementationsResponse> everywhereSymbolInfoFinder,
        GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse> genericIlSpyFindImplementationsCommandFactory)
    {
        _everywhereSymbolInfoFinder = everywhereSymbolInfoFinder;
        _genericIlSpyFindImplementationsCommandFactory = genericIlSpyFindImplementationsCommandFactory;
    }
        
    public override async Task<ResponsePacket<FindImplementationsResponse>> Handle(DecompiledLocationRequest request)
    {
        INavigationCommand<FindImplementationsResponse> command;
        if (!request.IsDecompiled)
        {
            command = await _everywhereSymbolInfoFinder.Get(request);
        }
        else
        {
            command = await _genericIlSpyFindImplementationsCommandFactory.Find(request);
        }
        
        var result = await command.Execute();
        return result;
    }
}