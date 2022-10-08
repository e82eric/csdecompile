using System;
using System.Threading.Tasks;
using CsDecompileLib.FindImplementations;

namespace CsDecompileLib.FindUsages;

public class DecompileFindUsagesHandler : HandlerBase<DecompiledLocationRequest, FindImplementationsResponse>
{
    private readonly EverywhereSymbolInfoFinder<FindImplementationsResponse> _everywhereSymbolInfoFinder;
    private readonly GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse> _assemblyLevelFindImplementationsCommandFactory;
    private readonly GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse> _classLevelFindImplementationsCommandFactory;

    public DecompileFindUsagesHandler(
        EverywhereSymbolInfoFinder<FindImplementationsResponse> everywhereSymbolInfoFinder,
        GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse> classLevelFindImplementationsCommandFactory,
        GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse> assemblyLevelFindImplementationsCommandFactory)
    {
        _everywhereSymbolInfoFinder = everywhereSymbolInfoFinder;
        _assemblyLevelFindImplementationsCommandFactory = assemblyLevelFindImplementationsCommandFactory;
        _classLevelFindImplementationsCommandFactory = classLevelFindImplementationsCommandFactory;
    }
        
    public override async Task<ResponsePacket<FindImplementationsResponse>> Handle(DecompiledLocationRequest request)
    {
        INavigationCommand<FindImplementationsResponse> command;
        switch (request.Type)
        {
            case LocationType.SourceCode:
                command = await _everywhereSymbolInfoFinder.Get(request);
                break;
            case LocationType.Decompiled:
                command = await _classLevelFindImplementationsCommandFactory.Find(request);
                break;
            case LocationType.DecompiledAssembly:
                command = await _assemblyLevelFindImplementationsCommandFactory.Find(request);
                break;
            default:
                throw new Exception("Unknown location type");
        }

        var result = await command.Execute();
        return result;
    }
}