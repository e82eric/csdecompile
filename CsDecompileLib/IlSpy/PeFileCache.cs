using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using ICSharpCode.Decompiler.Metadata;

namespace CsDecompileLib.IlSpy;

public class PeFileCache
{
    private readonly  ConcurrentDictionary<string, string> _byFileName = new();
    private readonly ConcurrentDictionary<string, PEFile> _peFileCache = new();

    public PEFile[] GetAssemblies()
    {
        var result = _peFileCache.Values.ToArray();
        return result;
    }

    public int GetAssemblyCount()
    {
        return _peFileCache.Count;
    }
    
    public bool TryGetByNameAndFrameworkId(string fullName, string targetFrameworkId, out PEFile peFile)
    {
        var uniqueness = fullName + '|' + targetFrameworkId;
        if (_peFileCache.TryGetValue(uniqueness, out peFile))
        {
            return true;
        }
        
        return false;
    }
        
    public bool TryGetFirstMatchByName(string fullName, out PEFile peFile)
    {
        peFile = null;
        var firstFulNameMatch = _peFileCache.Keys.FirstOrDefault(k => k.StartsWith(fullName));
        if(firstFulNameMatch == null)
        {
            return false;
        }
        if (_peFileCache.TryGetValue(firstFulNameMatch, out peFile))
        {
            return true;
        }

        return false;
    }
        
    public bool TryOpen(string fileName, out PEFile peFile)
    {
        if (_byFileName.TryGetValue(fileName, out var uniqueness))
        {
            if (_peFileCache.TryGetValue(uniqueness, out peFile))
            {
                return true;
            }
        }
            
        using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
            if(TryLoadAssembly(fileStream, PEStreamOptions.PrefetchEntireImage, fileName, out peFile))
            {
                var targetFrameworkId = peFile.DetectTargetFrameworkId();
                uniqueness = peFile.FullName + '|' + targetFrameworkId;
                _byFileName.TryAdd(fileName, uniqueness);
                _peFileCache.TryAdd(uniqueness, peFile);
                return true;
            }
            return false;
        }
    }
    
    private bool TryLoadAssembly(Stream stream, PEStreamOptions streamOptions, string fileName, out PEFile peFile)
    {
        peFile = null;
        try
        {
            var options = MetadataReaderOptions.ApplyWindowsRuntimeProjections;

            peFile = new PEFile(fileName, stream, streamOptions, metadataOptions: options);

            return true;
        }
        catch (Exception)
        {
            //TODO: Log something here
            return false;
        }
    }
}