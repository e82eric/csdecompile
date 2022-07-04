using System.IO;
using System.Threading.Tasks;

namespace TryOmnisharpExtension;

public interface IHandler
{
    Task<ResponsePacket> Handle(Stream argumentStream);
}