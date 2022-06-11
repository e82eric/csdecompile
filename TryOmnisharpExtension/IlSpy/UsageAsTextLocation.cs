using System.Reflection.Metadata;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace TryOmnisharpExtension.IlSpy;

public class UsageAsTextLocation
{
    public AstNode Node { get; set; }
    public EntityHandle TypeEntityHandle { get; set; }
    public TextLocation StartLocation { get; set; }
    public TextLocation EndLocation { get; set; }
    public string Statement { get; set; }
}
