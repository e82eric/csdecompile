using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;

namespace TryOmnisharpExtension.IlSpy;

[Shared]
[Export]
public class PeFileCache
{
    private readonly Dictionary<string, Dictionary<string, PEFile>> _peFileCache = new();
    private readonly  Dictionary<string, PEFile> _byFileName = new();

    public bool TryGetByNameAndFrameworkId(string fullName, string targetFrameworkId, out PEFile peFile)
    {
        if (_peFileCache.TryGetValue(fullName, out var moduleVersions))
        {
            if (moduleVersions.TryGetValue(targetFrameworkId, out peFile))
            {
                return true;
            }
        }

        peFile = null;
        return false;
    }
        
    public bool TryGetFirstMatchByName(string fullName, out PEFile peFile)
    {
        if (_peFileCache.TryGetValue(fullName, out var moduleVersions))
        {
            if (moduleVersions.Any())
            {
                peFile = moduleVersions.First().Value;
                return true;
            }
        }

        peFile = null;
        return false;
    }
        
    public async Task<PEFile> OpenAsync(string fileName)
    {
        if (_byFileName.TryGetValue(fileName, out var result))
        {
            return result;
        }
            
        using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
            result = LoadAssembly(fileStream, PEStreamOptions.PrefetchEntireImage, fileName);
            _byFileName.Add(fileName, result);
        }

        var targetFrameworkId = result.DetectTargetFrameworkId();
        if (!_peFileCache.TryGetValue(result.FullName, out var moduleVersions))
        {
            moduleVersions = new Dictionary<string, PEFile>
            {
                { targetFrameworkId, result }
            };
            _peFileCache.Add(result.FullName, moduleVersions);
        }
        else
        {
            if (!moduleVersions.ContainsKey(targetFrameworkId))
            {
                moduleVersions.Add(targetFrameworkId, result);
            }
        }

        return result;
    }
    
    private PEFile LoadAssembly(Stream stream, PEStreamOptions streamOptions, string fileName)
    {
        var options = MetadataReaderOptions.ApplyWindowsRuntimeProjections;

        PEFile module = new PEFile(fileName, stream, streamOptions, metadataOptions: options);

        return module;
    }
}