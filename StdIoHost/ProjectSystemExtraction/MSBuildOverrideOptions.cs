using System.Collections.Generic;

internal class MSBuildOverrideOptions
{
    public Dictionary<string, string> PropertyOverrides { get; set; }

    public string MSBuildPath { get; set; }

    public string Name { get; set; }
}