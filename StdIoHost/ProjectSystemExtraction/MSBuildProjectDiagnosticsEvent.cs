using System.Collections.Generic;

public class MSBuildProjectDiagnosticsEvent
{
    public const string Id = "MsBuildProjectDiagnostics";

    public string FileName { get; set; }
    public IEnumerable<MSBuildDiagnosticsMessage> Warnings { get; set; }
    public IEnumerable<MSBuildDiagnosticsMessage> Errors { get; set; }
}