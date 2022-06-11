namespace TryOmnisharpExtension;

public class SourceFileInfo : ResponseLocation
{
    public string TypeFullName { get; set; }
    public string NamespaceName { get; set; }
    public string FileName { get; set; }
    public override ResponseLocationType Type => ResponseLocationType.SourceCode;
}