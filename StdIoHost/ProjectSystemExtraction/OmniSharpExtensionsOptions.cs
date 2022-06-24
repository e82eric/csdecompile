using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StdIoHost.ProjectSystemExtraction;

public class OmniSharpExtensionsOptions
{
    public string[] LocationPaths { get; set; }

    public IEnumerable<string> GetNormalizedLocationPaths(string targetDirectory)
    {
        if (LocationPaths == null || LocationPaths.Length == 0) return Enumerable.Empty<string>();

        var normalizePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var locationPath in LocationPaths)
        {
            if (Path.IsPathRooted(locationPath))
            {
                normalizePaths.Add(locationPath);
            }
            else
            {
                normalizePaths.Add(Path.Combine(targetDirectory, locationPath));
            }
        }

        return normalizePaths;
    }
}