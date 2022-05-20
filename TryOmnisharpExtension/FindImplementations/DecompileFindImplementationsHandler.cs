using System.Composition;
using System.Threading.Tasks;
using OmniSharp.Mef;

namespace TryOmnisharpExtension;

[OmniSharpHandler(Endpoints.DecompileFindImplementations, Languages.Csharp), Shared]
public class DecompileFindImplementationsHandler : IRequestHandler<DecompileFindImplementationsRequest, FindImplementationsResponse>
{
    private readonly EverywhereSymbolInfoFinder2<FindImplementationsResponse> _everywhereSymbolInfoFinder2;
    private readonly IlSpyFindImplementationsCommandFactory2<FindImplementationsResponse> _ilSpyFindImplementationsCommandFactory2;

    [ImportingConstructor]
    public DecompileFindImplementationsHandler(
        EverywhereSymbolInfoFinder2<FindImplementationsResponse> everywhereSymbolInfoFinder2,
        IlSpyFindImplementationsCommandFactory2<FindImplementationsResponse> ilSpyFindImplementationsCommandFactory2)
    {
        _everywhereSymbolInfoFinder2 = everywhereSymbolInfoFinder2;
        _ilSpyFindImplementationsCommandFactory2 = ilSpyFindImplementationsCommandFactory2;
    }
        
    public async Task<FindImplementationsResponse> Handle(DecompileFindImplementationsRequest request)
    {
        INavigationCommand<FindImplementationsResponse> command;
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