namespace CsDecompileLib;

public abstract class ResponseLocation
{
    public string ContainingTypeShortName { get; set; }
    public string ContainingTypeFullName { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
    public string SourceText { get; set; }
    public abstract LocationType Type { get; }
}