namespace TryOmnisharpExtension;

public class SourceFileInfo : ResponseLocation
{
    public string FileName { get; set; }
    public override ResponseLocationType Type => ResponseLocationType.SourceCode;
}