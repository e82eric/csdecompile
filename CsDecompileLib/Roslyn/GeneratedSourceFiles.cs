using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace CsDecompileLib.Roslyn;

// Source generators ([GeneratedRegex], System.Text.Json, ...) add documents to the compilation that
// are never written to disk, although Roslyn reports a path for them under obj. An editor can't open
// that path, so the text is written to a file under the temp directory and that file is returned
// instead. The file name is derived from the reported path alone, so a request coming from the
// written file can be mapped back to the generated document without keeping any state.
public static class GeneratedSourceFiles
{
    private static readonly string Root = Path.Combine(Path.GetTempPath(), "csdecompile", "generated");

    public static bool IsWrittenFile(string filePath)
    {
        return !string.IsNullOrEmpty(filePath) &&
               Path.GetFullPath(filePath).StartsWith(Root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    public static string GetWrittenFilePath(string generatedDocumentPath)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(generatedDocumentPath.ToLowerInvariant()));
        var directory = Convert.ToHexString(hash, 0, 6);
        return Path.Combine(Root, directory, Path.GetFileName(generatedDocumentPath));
    }

    public static string Write(string generatedDocumentPath, SourceText text)
    {
        var filePath = GetWrittenFilePath(generatedDocumentPath);
        var content = text.ToString();
        if (!File.Exists(filePath) || File.ReadAllText(filePath) != content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        return filePath;
    }
}
