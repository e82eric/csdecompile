using System.Threading.Tasks;

namespace TryOmnisharpExtension.FindImplementations;

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
        var rosylynImplementationsTask = Task.Run(() => _rosylynFindImplementationsCommand.Execute());
        var ilSpyImplementationsTask = Task.Run(() => _ilSpyCommand.Execute());
        await Task.WhenAll(rosylynImplementationsTask, ilSpyImplementationsTask);

        var rosylynImplementations = rosylynImplementationsTask.Result;
        var ilSpyImplementations = ilSpyImplementationsTask.Result;

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