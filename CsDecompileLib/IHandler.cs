using System.IO;
using System.Threading.Tasks;

namespace CsDecompileLib;

public interface IHandler
{
    Task<ResponsePacket> Handle(Stream argumentStream);
}