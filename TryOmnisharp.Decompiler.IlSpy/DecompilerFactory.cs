using System.Collections.Concurrent;

namespace IlSpy.Analyzer.Extraction;

public static class DecompilerFactory
{
    private static readonly ConcurrentDictionary<string, CachingDecompiler> Decompilers;

    static DecompilerFactory()
    {
        Decompilers = new ConcurrentDictionary<string, CachingDecompiler>();
    }

    public static CachingDecompiler Get(string fileName)
    {
        if (Decompilers.TryGetValue(fileName, out var result))
        {
            return result;
        }

        result = new CachingDecompiler(fileName);
        Decompilers.TryAdd(fileName, result);
        return result;
    }
}