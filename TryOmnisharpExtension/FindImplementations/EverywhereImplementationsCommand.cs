using System.Threading.Tasks;

namespace TryOmnisharpExtension.FindImplementations;

public class EverywhereImplementationsCommand2<TResponseType> : INavigationCommand<TResponseType> where TResponseType : FindImplementationsResponse, new()
{
    private readonly INavigationCommand<TResponseType> _rosylynFindImplementationsCommand;
    private readonly INavigationCommand<TResponseType> _ilSpyCommand;

    public EverywhereImplementationsCommand2(
        INavigationCommand<TResponseType> rosylynFindImplementationsCommand,
        INavigationCommand<TResponseType> ilSpyCommand)
    {
        _rosylynFindImplementationsCommand = rosylynFindImplementationsCommand;
        _ilSpyCommand = ilSpyCommand;
    }
        
    public async Task<TResponseType> Execute()
    {
        var rosylynImplementationsTask = Task.Run(() => _rosylynFindImplementationsCommand.Execute());
        var ilSpyImplementationsTask = Task.Run(() => _ilSpyCommand.Execute());
        await Task.WhenAll(rosylynImplementationsTask, ilSpyImplementationsTask);

        var rosylynImplementations = rosylynImplementationsTask.Result;
        var ilSpyImplementations = ilSpyImplementationsTask.Result;

        var result = new TResponseType();

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