public class CommandPacket<TArguments>
{
    public string Command { get; set; }
    public int Seq { get; set; }
    public TArguments Arguments { get; set; }
}