using System.Threading.Tasks;

namespace CsDecompileLib.FindImplementations;

public class EverywhereImplementationsCommand<TResponseType> : INavigationCommand<TResponseType> where TResponseType : FindImplementationsResponse, new()
{
    private readonly INavigationCommand<TResponseType> _rosylynFindImplementationsCommand;
    private readonly INavigationCommand<TResponseType> _ilSpyCommand;

    public EverywhereImplementationsCommand(
        INavigationCommand<TResponseType> rosylynFindImplementationsCommand,
        INavigationCommand<TResponseType> ilSpyCommand)
    {
        _rosylynFindImplementationsCommand = rosylynFindImplementationsCommand;
        _ilSpyCommand = ilSpyCommand;
    }
        
    public async Task<ResponsePacket<TResponseType>> Execute()
    {
        var rosylynImplementationsTask = Task.Run(() => _rosylynFindImplementationsCommand.Execute());
        var ilSpyImplementationsTask = Task.Run(() => _ilSpyCommand.Execute());
        await Task.WhenAll(rosylynImplementationsTask, ilSpyImplementationsTask);

        var rosylynImplementations = rosylynImplementationsTask.Result;
        var ilSpyImplementations = ilSpyImplementationsTask.Result;

        var body = new TResponseType();

        foreach (var rosylynImplementation in rosylynImplementations.Body.Implementations)
        {
            body.Implementations.Add(rosylynImplementation);
        }
        
        foreach (var ilSpyImplementation in ilSpyImplementations.Body.Implementations)
        {
            body.Implementations.Add(ilSpyImplementation);
        }

        var result = ResponsePacket.Ok(body);
        return result;
    }
}