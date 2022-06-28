using System.Threading.Tasks;

namespace TryOmnisharpExtension.FindImplementations;

public class DecompileFindImplementationsHandler
{
    private readonly EverywhereSymbolInfoFinder2<FindImplementationsResponse> _everywhereSymbolInfoFinder2;
    private readonly IlSpyFindImplementationsCommandFactory2<FindImplementationsResponse> _ilSpyFindImplementationsCommandFactory;

    public DecompileFindImplementationsHandler(
        EverywhereSymbolInfoFinder2<FindImplementationsResponse> everywhereSymbolInfoFinder2,
        IlSpyFindImplementationsCommandFactory2<FindImplementationsResponse> ilSpyFindImplementationsCommandFactory)
    {
        _everywhereSymbolInfoFinder2 = everywhereSymbolInfoFinder2;
        _ilSpyFindImplementationsCommandFactory = ilSpyFindImplementationsCommandFactory;
    }
        
    public async Task<FindImplementationsResponse> Handle(DecompiledLocationRequest request)
    {
        INavigationCommand<FindImplementationsResponse> command;
        if (!request.IsDecompiled)
        {
            command = await _everywhereSymbolInfoFinder2.Get(request);
        }
        else
        {
            command = await _ilSpyFindImplementationsCommandFactory.Find(request);
        }
        
        var result = await command.Execute();
        return result;
    }
}