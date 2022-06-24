namespace StdIoHost.ProjectSystemExtraction;

public class StdioEventEmitter : IEventEmitter
{
    private readonly SharedTextWriter _writer;

    public StdioEventEmitter(SharedTextWriter writer)
    {
        _writer = writer;
    }

    public void Emit(string kind, object args)
    {
        var packet = new EventPacket
        {
            Event = kind,
            Body = args
        };

        _writer.WriteLine(packet);
    }
}