using StdIoHost;

namespace CsDecompileLib
{
    public class ResponsePacket<T> : ResponsePacket
    {
        public T Body { get; set; }
    }
    public class ResponsePacket: Packet
    {
        public static ResponsePacket<T> Ok<T>(T body)
        {
            var result = new ResponsePacket<T>()
            {
                Success = true,
                Body = body
            };
            return result;
        }
        public int Request_seq { get; set; }

        public string Command { get; set; }

        public bool Running { get; set; }

        public bool Success { get; set; }

        public string Message { get; set; }

        public ResponsePacket() : base("response")
        {
            Seq = _seqPool++;
        }
    }
}