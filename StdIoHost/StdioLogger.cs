using Microsoft.Extensions.Logging;

namespace StdIoHost;

class StdioLogger : BaseLogger
{
    private readonly SharedTextWriter _writer;

    public StdioLogger(SharedTextWriter writer, string categoryName)
        : base(categoryName, addHeader: false)
    {
        _writer = writer;
    }

    protected override void WriteMessage(LogLevel logLevel, string message)
    {
        var packet = new EventPacket
        {
            Event = "log",
            Body = new
            {
                LogLevel = logLevel.ToString().ToUpperInvariant(),
                Name = this.CategoryName,
                Message = message
            }
        };

        _writer.WriteLine(packet);
    }
}