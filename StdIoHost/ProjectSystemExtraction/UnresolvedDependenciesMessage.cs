using System.Collections.Generic;

public class UnresolvedDependenciesMessage
{
    public string FileName { get; set; }

    public IEnumerable<OmniSharp.Models.Events.PackageDependency> UnresolvedDependencies { get; set; }
}