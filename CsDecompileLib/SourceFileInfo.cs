namespace CsDecompileLib;

public class SourceFileInfo : ResponseLocation
{
    public string FileName { get; set; }
    public override LocationType Type => LocationType.SourceCode;
}