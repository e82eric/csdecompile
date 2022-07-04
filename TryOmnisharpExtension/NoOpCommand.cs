using System.Threading.Tasks;

namespace TryOmnisharpExtension;

public class NoOpCommand<T> : INavigationCommand<T> where T: FindImplementationsResponse, new()
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