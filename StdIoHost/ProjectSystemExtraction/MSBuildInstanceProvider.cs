using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

internal abstract class MSBuildInstanceProvider
{
    protected readonly ILogger Logger;
    protected static readonly ImmutableArray<MSBuildInstance> NoInstances = ImmutableArray<MSBuildInstance>.Empty;

    protected MSBuildInstanceProvider(ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger(this.GetType());
    }

    public abstract ImmutableArray<MSBuildInstance> GetInstances();

    protected static Version GetMSBuildVersion(string microsoftBuildPath)
    {
        var msbuildVersionInfo = FileVersionInfo.GetVersionInfo(microsoftBuildPath);
        var semanticVersion = SemanticVersion.Parse(msbuildVersionInfo.ProductVersion);
        return new Version(
            semanticVersion.Major,
            semanticVersion.Minor,
            semanticVersion.Patch
        );
    }
}