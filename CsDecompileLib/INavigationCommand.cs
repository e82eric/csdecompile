using System.Threading.Tasks;

namespace CsDecompileLib;

public interface INavigationCommand<T>
{
    Task<ResponsePacket<T>> Execute();
}