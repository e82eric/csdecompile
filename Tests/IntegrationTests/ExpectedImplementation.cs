using CsDecompileLib;

namespace IntegrationTests;

public class ExpectedImplementation
{
    public ExpectedImplementation(LocationType type, string line,string shortName, string fullName)
    {
        Type = type;
        ShortName = shortName;
        Line = line;
        FullName = fullName;
    }

    public LocationType Type { get; }
    public string ShortName { get; }
    public string Line { get; }
    public string FullName { get; }
}