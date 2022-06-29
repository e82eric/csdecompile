using System.IO;
using System.Threading.Tasks;

namespace TryOmnisharpExtension;

public interface IHandler
{
    Task<object> Handle(Stream argumentStream);
}