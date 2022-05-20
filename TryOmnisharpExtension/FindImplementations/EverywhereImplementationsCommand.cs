using System.Threading.Tasks;

namespace TryOmnisharpExtension;

public class EverywhereImplementationsCommand : IFindImplementationsCommand
{
    private readonly IFindImplementationsCommand _rosylynFindImplementationsCommand;
    private readonly IFindImplementationsCommand _ilSpyCommand;

    public EverywhereImplementationsCommand(
        IFindImplementationsCommand rosylynFindImplementationsCommand,
        IFindImplementationsCommand ilSpyCommand)
    {
        _rosylynFindImplementationsCommand = rosylynFindImplementationsCommand;
        _ilSpyCommand = ilSpyCommand;
    }
        
    public async Task<FindImplementationsResponse> Execute()
    {
        var rosylynImplementations = await _rosylynFindImplementationsCommand.Execute();
        var ilSpyImplementations = await _ilSpyCommand.Execute();

        var result = new FindImplementationsResponse();

        foreach (var rosylynImplementation in rosylynImplementations.Implementations)
        {
            result.Implementations.Add(rosylynImplementation);
        }
        
        foreach (var ilSpyImplementation in ilSpyImplementations.Implementations)
        {
            result.Implementations.Add(ilSpyImplementation);
        }

        return result;
    }
}

public class EverywhereImplementationsCommand2<ResponseType> : INavigationCommand<ResponseType> where ResponseType : FindImplementationsResponse, new()
{
    private readonly INavigationCommand<ResponseType> _rosylynFindImplementationsCommand;
    private readonly INavigationCommand<ResponseType> _ilSpyCommand;

    public EverywhereImplementationsCommand2(
        INavigationCommand<ResponseType> rosylynFindImplementationsCommand,
        INavigationCommand<ResponseType> ilSpyCommand)
    {
        _rosylynFindImplementationsCommand = rosylynFindImplementationsCommand;
        _ilSpyCommand = ilSpyCommand;
    }
        
    public async Task<ResponseType> Execute()
    {
        var rosylynImplementations = await _rosylynFindImplementationsCommand.Execute();
        var ilSpyImplementations = await _ilSpyCommand.Execute();

        var result = new ResponseType();

        foreach (var rosylynImplementation in rosylynImplementations.Implementations)
        {
            result.Implementations.Add(rosylynImplementation);
        }
        
        foreach (var ilSpyImplementation in ilSpyImplementations.Implementations)
        {
            result.Implementations.Add(ilSpyImplementation);
        }

        return result;
    }
}