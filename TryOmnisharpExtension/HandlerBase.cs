using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TryOmnisharpExtension;

public abstract class HandlerBase<TRequest, TResponse> : IHandler
{
    public abstract Task<ResponsePacket<TResponse>> Handle(TRequest request);

    public async Task<ResponsePacket> Handle(Stream argumentStream)
    {
        var requestObject = GetRequestObject(argumentStream);
        var result = await Handle(requestObject);
        return result;
    }
    
    private TRequest GetRequestObject(Stream argumentStream)
    {
        var arguments = DeserializeRequestObject(argumentStream);
        var argObject = arguments.ToObject<TRequest>();
        return argObject;
    }
    
    private JToken DeserializeRequestObject(Stream readStream)
    {
        try
        {
            using (var streamReader = new StreamReader(readStream))
            {
                using (var textReader = new JsonTextReader(streamReader))
                {
                    return JToken.Load(textReader);
                }
            }
        }
        catch
        {
            return new JObject();
        }
    }
}