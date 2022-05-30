using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TryOmnisharpExtension.GetMembers;

namespace StdIoHost
{
    public class GetAllTypesRequest
    {
        public string Directory { get; set; }
        public string TypeName { get; set; }
    }
    
    public class AllTypesHandler : EndpointHandler
    {
        private readonly IlSpyAllTypesRepository _allTypesRepository;

        public AllTypesHandler(IlSpyAllTypesRepository allTypesRepository)
        {
            _allTypesRepository = allTypesRepository;
        }
        public override async Task<object> Handle(RequestPacket packet)
        {
            var arguments = DeserializeRequestObject(packet.ArgumentsStream);

            var argObject = arguments.ToObject<GetAllTypesRequest>();
            
            var allTypes = await _allTypesRepository.GetAllTypes(
                argObject.Directory,
                argObject.TypeName);

            var result = new { AllTypes = allTypes };
            return result;
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
}