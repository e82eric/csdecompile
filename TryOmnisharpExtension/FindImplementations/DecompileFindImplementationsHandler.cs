using System.Composition;
using System.Threading.Tasks;
using Autofac;
using OmniSharp.Mef;

namespace TryOmnisharpExtension;

[OmniSharpHandler(Endpoints.DecompileFindImplementations, Languages.Csharp), Shared]
public class DecompileFindImplementationsHandler : IRequestHandler<DecompileFindImplementationsRequest, FindImplementationsResponse>
{
    private readonly EverywhereSymbolInfoFinder2<FindImplementationsResponse> _everywhereSymbolInfoFinder2;
    private readonly IlSpyFindImplementationsCommandFactory2<FindImplementationsResponse> _ilSpyFindImplementationsCommandFactory2;
    private IlSpyExternalAssembliesCommandFactory<FindImplementationsResponse> _externalAssemblyCommandFactory;

    [ImportingConstructor]
    public DecompileFindImplementationsHandler(
        EverywhereSymbolInfoFinder2<FindImplementationsResponse> everywhereSymbolInfoFinder2,
        IlSpyFindImplementationsCommandFactory2<FindImplementationsResponse> ilSpyFindImplementationsCommandFactory2,
        ExtensionContainer extensionContainer)
    {
        _everywhereSymbolInfoFinder2 = everywhereSymbolInfoFinder2;
        _ilSpyFindImplementationsCommandFactory2 = ilSpyFindImplementationsCommandFactory2;
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
                command = await _externalAssemblyCommandFactory.Find(request);
            }
            else
            {
                command = await _ilSpyFindImplementationsCommandFactory2.Find(request);
            }
        }
        
        var result = await command.Execute();
        return result;
    }
}