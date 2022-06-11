using System.Collections.Generic;
using System.Composition;
using System.IO;

namespace TryOmnisharpExtension.ExternalAssemblies;

[Shared]
[Export]
public class ExternalAssembliesWorkspace
{
    private readonly List<DirectoryInfo> _directoriesToSearch = new();
    
    public void AddDirectory(string directoryFilePath)
    {
        var directoryInfo = new DirectoryInfo(directoryFilePath);
        _directoriesToSearch.Add(directoryInfo);
    }

    public List<DirectoryInfo> GetDirectories()
    {
        return _directoriesToSearch;
    }
}