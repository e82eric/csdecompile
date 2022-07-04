using System.Threading.Tasks;

namespace TryOmnisharpExtension;

public interface INavigationCommand<T>
{
    Task<ResponsePacket<T>> Execute();
}