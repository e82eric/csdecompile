using System.Composition;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class PropertyInTypeFinder2
{
    private readonly DecompilerFactory _decompilerFactory;

    [ImportingConstructor]
    public PropertyInTypeFinder2(DecompilerFactory decompilerFactory)
    {
        _decompilerFactory = decompilerFactory;
    }
    
    public async Task<(UsageAsTextLocation, string)> Find(
        EntityHandle? setterEntityHandle,
        EntityHandle? getterEntityHandle,
        ITypeDefinition rootType)
    {
        var assemblyFilePath = rootType.ParentModule.PEFile.FileName;
        var decompiler = await _decompilerFactory.Get(assemblyFilePath);
        var (syntaxTree, sourceText) = decompiler.Run(rootType);

        var result = Find(syntaxTree, setterEntityHandle, getterEntityHandle, rootType.MetadataToken);

        return (result, sourceText);
    }

    private UsageAsTextLocation Find(AstNode node, EntityHandle? setterEntityHandle, EntityHandle? getterEntityHandle, EntityHandle rootTypeEntityHandle)
    {
        var symbol = node.GetSymbol();

        if (symbol != null)
        {
            if (symbol is IProperty entity)
            {
                if (entity.Setter != null && setterEntityHandle != null)
                {
                    if (entity.Setter.MetadataToken == setterEntityHandle)
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

                if (entity.Getter != null && getterEntityHandle != null)
                {
                    if (entity.Getter.MetadataToken == getterEntityHandle)
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
        }

        foreach (var child in node.Children)
        {
            var usage = Find(child, setterEntityHandle, getterEntityHandle, rootTypeEntityHandle);
            if (usage != null)
            {
                return usage;
            }
        }

        return null;
    }
}
