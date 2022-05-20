using System.Composition;
using System.Threading.Tasks;
using OmniSharp.Mef;
using TryOmnisharpExtension.FindUsages;

namespace TryOmnisharpExtension;

[OmniSharpHandler(Endpoints.DecompileFindUsages, Languages.Csharp), Shared]
public class DecompileFindUsagesHandler : IRequestHandler<DecompileFindUsagesRequest, FindUsagesResponse>
{
    private readonly EverywhereSymbolInfoFinder2<FindUsagesResponse> _everywhereSymbolInfoFinder2;
    private readonly IlSpyFindImplementationsCommandFactory2<FindUsagesResponse> _ilSpyFindImplementationsCommandFactory2;

    [ImportingConstructor]
    public DecompileFindUsagesHandler(
        EverywhereSymbolInfoFinder2<FindUsagesResponse> everywhereSymbolInfoFinder2,
        IlSpyFindImplementationsCommandFactory2<FindUsagesResponse> ilSpyFindImplementationsCommandFactory2)
    {
        _everywhereSymbolInfoFinder2 = everywhereSymbolInfoFinder2;
        _ilSpyFindImplementationsCommandFactory2 = ilSpyFindImplementationsCommandFactory2;
    }
        
    public async Task<FindUsagesResponse> Handle(DecompileFindUsagesRequest request)
    {
        INavigationCommand<FindUsagesResponse> command;
        if (!request.IsDecompiled)
        {
            command = await _everywhereSymbolInfoFinder2.Get(request);
        }
        else
        {
            command = await _ilSpyFindImplementationsCommandFactory2.Find(request);
        }
        
        var result = await command.Execute();
        return result;
    }
}
