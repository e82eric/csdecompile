using System.Threading.Tasks;
using TryOmnisharpExtension.FindImplementations;

namespace TryOmnisharpExtension.FindUsages;

public class DecompileFindUsagesHandler : HandlerBase<DecompiledLocationRequest, FindUsagesResponse>
{
    private readonly EverywhereSymbolInfoFinder2<FindUsagesResponse> _everywhereSymbolInfoFinder;
    private readonly GenericIlSpyFindImplementationsCommandFactory<FindUsagesResponse> _genericIlSpyFindImplementationsCommandFactory;

    public DecompileFindUsagesHandler(
        EverywhereSymbolInfoFinder2<FindUsagesResponse> everywhereSymbolInfoFinder,
        GenericIlSpyFindImplementationsCommandFactory<FindUsagesResponse> genericIlSpyFindImplementationsCommandFactory)
    {
        _everywhereSymbolInfoFinder = everywhereSymbolInfoFinder;
        _genericIlSpyFindImplementationsCommandFactory = genericIlSpyFindImplementationsCommandFactory;
    }
        
    public override async Task<FindUsagesResponse> Handle(DecompiledLocationRequest request)
    {
        INavigationCommand<FindUsagesResponse> command;
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
