using System.Threading.Tasks;
using Autofac;
using OmniSharp.Mef;

namespace TryOmnisharpExtension.FindImplementations;

public class DecompileFindImplementationsHandler : IRequestHandler<DecompileFindImplementationsRequest, FindImplementationsResponse>
{
    private readonly EverywhereSymbolInfoFinder2<FindImplementationsResponse> _everywhereSymbolInfoFinder2;
    private readonly IlSpyFindImplementationsCommandFactory2<FindImplementationsResponse> _ilSpyFindImplementationsCommandFactory;
    private IlSpyExternalAssembliesCommandFactory<FindImplementationsResponse> _externalAssemblyCommandFactory;

    public DecompileFindImplementationsHandler(
        EverywhereSymbolInfoFinder2<FindImplementationsResponse> everywhereSymbolInfoFinder2,
        IlSpyFindImplementationsCommandFactory2<FindImplementationsResponse> ilSpyFindImplementationsCommandFactory,
        ExtensionContainer extensionContainer)
    {
        _everywhereSymbolInfoFinder2 = everywhereSymbolInfoFinder2;
        _ilSpyFindImplementationsCommandFactory = ilSpyFindImplementationsCommandFactory;
        _externalAssemblyCommandFactory = extensionContainer.Container
            .Resolve<IlSpyExternalAssembliesCommandFactory<FindImplementationsResponse>>();
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
            if (request.IsFromExternalAssembly)
            {
                command = _externalAssemblyCommandFactory.Find(request);
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