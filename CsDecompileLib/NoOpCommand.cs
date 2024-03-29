using System.Threading.Tasks;

namespace CsDecompileLib;

public class NoOpCommand<T> : INavigationCommand<T> where T: LocationsResponse, new()
{
    public Task<ResponsePacket<T>> Execute()
    {
        var response = new ResponsePacket<T>()
        {
            Success = true
        };
        return Task.FromResult(response);
    }
}