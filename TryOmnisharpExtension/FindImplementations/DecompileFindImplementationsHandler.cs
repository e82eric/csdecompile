using System.Threading.Tasks;

namespace TryOmnisharpExtension.FindImplementations;

public class DecompileFindImplementationsHandler : HandlerBase<DecompiledLocationRequest, FindImplementationsResponse>
{
    private readonly EverywhereSymbolInfoFinder2<FindImplementationsResponse> _everywhereSymbolInfoFinder2;
    private readonly GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse> _genericIlSpyFindImplementationsCommandFactory;

    public DecompileFindImplementationsHandler(
        EverywhereSymbolInfoFinder2<FindImplementationsResponse> everywhereSymbolInfoFinder2,
        GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse> genericIlSpyFindImplementationsCommandFactory)
    {
        _everywhereSymbolInfoFinder2 = everywhereSymbolInfoFinder2;
        _genericIlSpyFindImplementationsCommandFactory = genericIlSpyFindImplementationsCommandFactory;
    }
        
    public override async Task<FindImplementationsResponse> Handle(DecompiledLocationRequest request)
    {
        INavigationCommand<FindImplementationsResponse> command;
        if (!request.IsDecompiled)
        {
            command = await _everywhereSymbolInfoFinder2.Get(request);
        }
        else
        {
            command = await _genericIlSpyFindImplementationsCommandFactory.Find(request);
        }
        
        var result = await command.Execute();
        return result;
    }
}