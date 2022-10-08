using System;
using System.Threading.Tasks;

namespace CsDecompileLib;

public class NavigationHandlerBase<TRequest, TResponse> : HandlerBase<TRequest, TResponse> where TRequest : DecompiledLocationRequest 
{
    private readonly INavigationCommandFactoryAsync<INavigationCommand<TResponse>, DecompiledLocationRequest> _rosylnGotoDefinitionCommandFactory;
    private readonly INavigationCommandFactory<INavigationCommand<TResponse>> _classLevelCommandFactory;
    private readonly INavigationCommandFactory<INavigationCommand<TResponse>> _assemblyLevelCommandFactory;

    public NavigationHandlerBase(
        INavigationCommandFactoryAsync<INavigationCommand<TResponse>, DecompiledLocationRequest> roslynLocationToCommandFactory,
        INavigationCommandFactory<INavigationCommand<TResponse>> classLevelCommandFactory,
        INavigationCommandFactory<INavigationCommand<TResponse>> assemblyLevelCommandFactory)
    {
        _rosylnGotoDefinitionCommandFactory = roslynLocationToCommandFactory;
        _classLevelCommandFactory = classLevelCommandFactory;
        _assemblyLevelCommandFactory = assemblyLevelCommandFactory;
    }
    
    public override async Task<ResponsePacket<TResponse>> Handle(TRequest request)
    {
        INavigationCommand<TResponse> command;

        switch (request.Type)
        {
            case LocationType.SourceCode:
                command = await _rosylnGotoDefinitionCommandFactory.Get(request);
                break;
            case LocationType.Decompiled:
                command = _classLevelCommandFactory.Find(request);
                break;
            case LocationType.DecompiledAssembly:
                command = _assemblyLevelCommandFactory.Find(request);
                break;
            default:
                throw new Exception("Unknown Location Type");
        }
        
        var result = await command.Execute();
        return result;
    }
}