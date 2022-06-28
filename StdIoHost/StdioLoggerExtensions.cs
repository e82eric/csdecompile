using Microsoft.Extensions.Logging;

namespace StdIoHost.ProjectSystemExtraction;

static class StdioLoggerExtensions
{
    public static ILoggingBuilder AddStdio(this ILoggingBuilder builder, SharedTextWriter writer)
    {
        builder.AddProvider(new StdioLoggerProvider(writer));
        return builder;
    }
}