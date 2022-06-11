using System.Composition;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class MethodInTypeFinder2
{
    private readonly DecompilerFactory _decompilerFactory;

    [ImportingConstructor]
    public MethodInTypeFinder2(DecompilerFactory decompilerFactory)
    {
        _decompilerFactory = decompilerFactory;
    }
    
    public (UsageAsTextLocation, string) Find(
        EntityHandle handleToSearchFor,
        ITypeDefinition rootTypeHandle)
    {
        var assemblyFilePath = rootTypeHandle.ParentModule.PEFile.FileName;
        var decompiler = _decompilerFactory.Get(assemblyFilePath);
        var (syntaxTree, sourceText) = decompiler.Run(rootTypeHandle);

        var usage = Find(syntaxTree, handleToSearchFor);

        return (usage, sourceText);
    }

    private UsageAsTextLocation Find(AstNode node, EntityHandle handleToSearchFor)
    {
        var symbol = node.GetSymbol();

        if (symbol != null)
        {
            if(symbol is IEntity entity)
            {
                if ( handleToSearchFor == entity.MetadataToken && node.NodeType == NodeType.Member)
                {
                    var identifier = node.Children.Where(n =>
                    {
                        var entity = n as Identifier;
                        if (entity != null)
                        {
                            if (entity.Name == symbol.Name)
                            {
                                return true;
                            }
                        }

                        return false;
                    }).FirstOrDefault();

                    if (identifier != null)
                    {
                        var usage = new UsageAsTextLocation()
                        {
                            StartLocation = identifier.StartLocation,
                            EndLocation = identifier.EndLocation,
                            Statement = identifier.ToString()
                        };

                        return usage;
                    }
                    else
                    {
                        var usage = new UsageAsTextLocation
                        {
                            StartLocation = node.StartLocation,
                            EndLocation = node.StartLocation,
                            Statement = "node.ToString()"
                        };

                        return usage;
                    }
                }
            }
        }

        foreach (var child in node.Children)
        {
            var usage = Find(child, handleToSearchFor);
            if (usage != null)
            {
                return usage;
            }
        }

        return null;
    }
}