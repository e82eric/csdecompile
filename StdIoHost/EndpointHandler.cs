using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StdIoHost
{
    public abstract class EndpointHandler
    {
        public abstract Task<object> Handle(RequestPacket packet);
    }
}