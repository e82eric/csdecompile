using Microsoft.Extensions.Logging;
using OmniSharp.Services;

namespace StdIoHost.ProjectSystemExtraction;

static class StdioLoggerExtensions
{
    public static ILoggingBuilder AddStdio(this ILoggingBuilder builder, ISharedTextWriter writer)
    {
        builder.AddProvider(new StdioLoggerProvider(writer));
        return builder;
    }
}