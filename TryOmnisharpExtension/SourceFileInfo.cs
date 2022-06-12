namespace TryOmnisharpExtension;

public class SourceFileInfo : ResponseLocation
{
    public string ContainingTypeFullName { get; set; }
    public string NamespaceName { get; set; }
    public string FileName { get; set; }
    public override ResponseLocationType Type => ResponseLocationType.SourceCode;
}