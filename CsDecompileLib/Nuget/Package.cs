namespace CsDecompileLib.Nuget;

public class Package
{
    public string Identity { get; set; }
    public string Version { get; set; }
    public int MajorVersion { get; set; }
    public int MinorVersion { get; set; }
    public int Build { get; set; }
    public int Revision { get; set; }
    public int Patch { get; set; }
}