using System.Threading.Tasks;
using TryOmnisharpExtension.FindImplementations;

namespace TryOmnisharpExtension.FindUsages;

public class DecompileFindUsagesHandler
{
    private readonly EverywhereSymbolInfoFinder2<FindUsagesResponse> _everywhereSymbolInfoFinder;
    private readonly IlSpyFindImplementationsCommandFactory2<FindUsagesResponse> _ilSpyFindImplementationsCommandFactory;

    public DecompileFindUsagesHandler(
        EverywhereSymbolInfoFinder2<FindUsagesResponse> everywhereSymbolInfoFinder,
        IlSpyFindImplementationsCommandFactory2<FindUsagesResponse> ilSpyFindImplementationsCommandFactory)
    {
        _everywhereSymbolInfoFinder = everywhereSymbolInfoFinder;
        _ilSpyFindImplementationsCommandFactory = ilSpyFindImplementationsCommandFactory;
    }
        
    public async Task<FindUsagesResponse> Handle(DecompiledLocationRequest request)
    {
        INavigationCommand<FindUsagesResponse> command;
        if (!request.IsDecompiled)
        {
            command = await _everywhereSymbolInfoFinder.Get(request);
        }
        else
        {
            command = await _ilSpyFindImplementationsCommandFactory.Find(request);
        }
        
        var result = await command.Execute();
        return result;
    }
}
