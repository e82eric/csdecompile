using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using Microsoft.Extensions.FileSystemGlobbing;

public class FileSystemHelper
{
    private static string s_directorySeparatorChar = Path.DirectorySeparatorChar.ToString();

    [ImportingConstructor]
    public FileSystemHelper()
    {
    }

    public static string GetRelativePath(string fullPath, string basePath)
    {
        // if any of them is not set, abort
        if (string.IsNullOrWhiteSpace(basePath) || string.IsNullOrWhiteSpace(fullPath)) return null;

        // paths must be rooted
        if (!Path.IsPathRooted(basePath) || !Path.IsPathRooted(fullPath)) return null;

        // if they are the same, abort
        if (fullPath.Equals(basePath, StringComparison.Ordinal)) return null;

        if (!Path.HasExtension(basePath) && !basePath.EndsWith(s_directorySeparatorChar))
        {
            basePath += Path.DirectorySeparatorChar;
        }

        var baseUri = new Uri(basePath);
        var fullUri = new Uri(fullPath);
        var relativeUri = baseUri.MakeRelativeUri(fullUri);
        var relativePath = Uri.UnescapeDataString(relativeUri.ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        return relativePath;
    }
}