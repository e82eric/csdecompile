namespace TryOmnisharpExtension;

public abstract class ResponseLocation
{
    public int Line { get; set; }
    public int Column { get; set; }
    public string SourceText { get; set; }
    public abstract ResponseLocationType Type { get; }
}