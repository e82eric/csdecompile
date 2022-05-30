using System.Composition;
using System.Threading.Tasks;
using Autofac;
using OmniSharp.Mef;
using TryOmnisharpExtension.FindUsages;

namespace TryOmnisharpExtension;

[OmniSharpHandler(Endpoints.DecompileFindUsages, Languages.Csharp), Shared]
public class DecompileFindUsagesHandler : IRequestHandler<DecompileFindUsagesRequest, FindUsagesResponse>
{
    private readonly EverywhereSymbolInfoFinder2<FindUsagesResponse> _everywhereSymbolInfoFinder2;
    private readonly IlSpyFindImplementationsCommandFactory2<FindUsagesResponse> _ilSpyFindImplementationsCommandFactory2;
    private IlSpyExternalAssembliesCommandFactory<FindUsagesResponse> _externalAssembliesSybolFinder;

    [ImportingConstructor]
    public DecompileFindUsagesHandler(
        EverywhereSymbolInfoFinder2<FindUsagesResponse> everywhereSymbolInfoFinder2,
        IlSpyFindImplementationsCommandFactory2<FindUsagesResponse> ilSpyFindImplementationsCommandFactory2,
        ExtensionContainer extensionContainer)
    {
        _everywhereSymbolInfoFinder2 = everywhereSymbolInfoFinder2;
        _ilSpyFindImplementationsCommandFactory2 = ilSpyFindImplementationsCommandFactory2;
        _externalAssembliesSybolFinder = extensionContainer.Container
            .Resolve<IlSpyExternalAssembliesCommandFactory<FindUsagesResponse>>();
        
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
            if (request.IsFromExternalAssembly)
            {
                command = await _externalAssembliesSybolFinder.Find(request);
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
