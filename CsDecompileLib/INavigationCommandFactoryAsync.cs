using System.Threading.Tasks;

namespace CsDecompileLib;

public interface INavigationCommandFactoryAsync<TCommandType, TRequest>
{
    Task<TCommandType> Get(TRequest request);
}