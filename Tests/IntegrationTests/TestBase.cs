using System.IO;
using TryOmnisharpExtension;

namespace IntegrationTests;

public class TestBase
{
    protected static string[] InSourceGetLines(ResponseLocation implementation)
    {
        var sourceFileInfo = (SourceFileInfo)implementation;
        var lines = File.ReadAllLines(sourceFileInfo.FileName);
        return lines;
    }
}