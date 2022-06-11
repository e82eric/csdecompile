using System.Composition;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class EventInTypeFinder
{
    private readonly DecompilerFactory _decompilerFactory;

    [ImportingConstructor]
    public EventInTypeFinder(DecompilerFactory decompilerFactory)
    {
        _decompilerFactory = decompilerFactory;
    }
    
    public (UsageAsTextLocation, string) Find(
        EntityHandle? eventEntityHandle,
        ITypeDefinition rootType)
    {
        var assemblyFilePath = rootType.ParentModule.PEFile.FileName;
        var decompiler = _decompilerFactory.Get(assemblyFilePath);
        var (syntaxTree, sourceText) = decompiler.Run(rootType);

        var result = Find(syntaxTree, eventEntityHandle, rootType.MetadataToken);

        return (result, sourceText);
    }

    private UsageAsTextLocation Find(AstNode node, EntityHandle? eventEntityHandle,  EntityHandle rootTypeEntityHandle)
    {
        var symbol = node.GetSymbol();

        if (symbol != null)
        {
            if (symbol is IEvent entity)
            {
                if (entity.MetadataToken == eventEntityHandle)
                {
                    var usage = new UsageAsTextLocation()
                    {
                        TypeEntityHandle = rootTypeEntityHandle,
                        StartLocation = node.StartLocation,
                        EndLocation = node.EndLocation,
                        Statement = node.Parent.ToString()
                    };

                    return usage;
                }
            }
        }

        foreach (var child in node.Children)
        {
            var usage = Find(child, eventEntityHandle, rootTypeEntityHandle);
            if (usage != null)
            {
                return usage;
            }
        }

        return null;
    }
}