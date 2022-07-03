using StdIoHost;

class CommandPacket<TArguments>
{
    public string Command { get; set; }
    public int Seq { get; set; }
    public TArguments Arguments { get; set; }
}

public class ResponsePacket2<T> : Packet
{
    public int Request_seq { get; set; }

    public string Command { get; set; }

    public bool Running { get; set; }

    public bool Success { get; set; }

    public string Message { get; set; }

    public T Body { get; set; }

    public ResponsePacket2() : base("response")
    {
        Seq = _seqPool++;
    }
}
