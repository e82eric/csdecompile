namespace TryOmnisharpExtension;

public class SourceFileInfo : ResponseLocation
{
    public string FileName { get; set; }
    public override ResponseLocationType Type => ResponseLocationType.SourceCode;
}

public abstract class ResponseLocation
{
    public int Line { get; set; }
    public int Column { get; set; }
    public string SourceText { get; set; }
    public abstract ResponseLocationType Type { get; }
}

public enum ResponseLocationType
{
    Decompiled,
    SourceCode
}
