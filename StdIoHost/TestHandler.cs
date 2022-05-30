using System;
using System.Threading.Tasks;

namespace StdIoHost
{
    public class TestHandler : EndpointHandler
    {
        private Random _random = new Random();
        public override async Task<object> Handle(RequestPacket packet)
        {
            var result = new { Msg = "Eric Response", Random = _random.Next() };
            return result;
        }
    }
}