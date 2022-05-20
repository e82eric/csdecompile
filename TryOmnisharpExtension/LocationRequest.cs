using OmniSharp;

namespace TryOmnisharpExtension;

public class LocationRequest : IRequest
{
    public bool IsDecompiled { get; set; }
    public string FileName { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
}