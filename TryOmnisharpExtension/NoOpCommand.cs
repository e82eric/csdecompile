using System.Threading.Tasks;
using TryOmnisharpExtension.FindImplementations;

namespace TryOmnisharpExtension;

public class NoOpCommand<T> : INavigationCommand<T> where T: FindImplementationsResponse, new()
{
    public Task<T> Execute()
    {
        return Task.FromResult(new T());
    }
}