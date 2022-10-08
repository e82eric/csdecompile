using System;
using System.Threading.Tasks;

namespace CsDecompileLib.FindImplementations;

public class DecompileFindImplementationsHandler : HandlerBase<DecompiledLocationRequest, FindImplementationsResponse>
{
    private readonly EverywhereSymbolInfoFinder<FindImplementationsResponse> _everywhereSymbolInfoFinder;
    private readonly GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse> _classLevelFindImplementationsCommandFactory;
    private readonly GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse> _assemblyLevelFindImplementationsCommandFactory;

    public DecompileFindImplementationsHandler(
        EverywhereSymbolInfoFinder<FindImplementationsResponse> everywhereSymbolInfoFinder,
        GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse> classLevelFindImplementationsCommandFactory,
        GenericIlSpyFindImplementationsCommandFactory<FindImplementationsResponse> assemblyLevelFindImplementationsCommandFactory)
    {
        _everywhereSymbolInfoFinder = everywhereSymbolInfoFinder;
        _classLevelFindImplementationsCommandFactory = classLevelFindImplementationsCommandFactory;
        _assemblyLevelFindImplementationsCommandFactory = assemblyLevelFindImplementationsCommandFactory;
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
                throw new Exception("Unknown Location Type");
        }
        
        var result = await command.Execute();
        return result;
    }
}