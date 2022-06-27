using System.Threading.Tasks;
using OmniSharp.Mef;
using TryOmnisharpExtension.FindImplementations;

namespace TryOmnisharpExtension.FindUsages;

public class DecompileFindUsagesHandler : IRequestHandler<DecompileFindUsagesRequest, FindUsagesResponse>
{
    private readonly EverywhereSymbolInfoFinder2<FindUsagesResponse> _everywhereSymbolInfoFinder;
    private readonly IlSpyFindImplementationsCommandFactory2<FindUsagesResponse> _ilSpyFindImplementationsCommandFactory;
    private IlSpyExternalAssembliesCommandFactory<FindUsagesResponse> _externalAssembliesSybolFinder;

    public DecompileFindUsagesHandler(
        EverywhereSymbolInfoFinder2<FindUsagesResponse> everywhereSymbolInfoFinder,
        IlSpyFindImplementationsCommandFactory2<FindUsagesResponse> ilSpyFindImplementationsCommandFactory,
        IlSpyExternalAssembliesCommandFactory<FindUsagesResponse> externalAssembliesSybolFinder)
    {
        _everywhereSymbolInfoFinder = everywhereSymbolInfoFinder;
        _ilSpyFindImplementationsCommandFactory = ilSpyFindImplementationsCommandFactory;
        _externalAssembliesSybolFinder = externalAssembliesSybolFinder;
    }
        
    public async Task<FindUsagesResponse> Handle(DecompileFindUsagesRequest request)
    {
        INavigationCommand<FindUsagesResponse> command;
        if (!request.IsDecompiled)
        {
            command = await _everywhereSymbolInfoFinder.Get(request);
        }
        else
        {
            if (request.IsFromExternalAssembly)
            {
                command = _externalAssembliesSybolFinder.Find(request);
            }
            else
            {
                command = await _ilSpyFindImplementationsCommandFactory.Find(request);
            }
        }
        
        var result = await command.Execute();
        return result;
    }
}
