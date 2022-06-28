using Microsoft.Extensions.Logging;

namespace StdIoHost;

class StdioLoggerProvider : ILoggerProvider
{
    private readonly SharedTextWriter _writer;

    public StdioLoggerProvider(SharedTextWriter writer)
    {
        _writer = writer;
    }

    public ILogger CreateLogger(string name)
    {
        return new StdioLogger(_writer, name);
    }

    public void Dispose() { }
}