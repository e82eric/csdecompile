using System.Collections.Generic;
using System.Composition;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class TypeUsedInTypeFinder
{
    private readonly DecompilerFactory _decompilerFactory;

    [ImportingConstructor]
    public TypeUsedInTypeFinder(DecompilerFactory decompilerFactory)
    {
        _decompilerFactory = decompilerFactory;
    }
    
    public async Task<IEnumerable<UsageAsTextLocation>> Find2(ITypeDefinition symbol, EntityHandle typeEntityHandle)
    {
        var assemblyFilePath = symbol.ParentModule.PEFile.FileName;
        var decompiler = await _decompilerFactory.Get(assemblyFilePath);
        var (syntaxTree, source) = decompiler.Run(symbol);

        var result = new List<UsageAsTextLocation>();
        Find2(syntaxTree, typeEntityHandle, result);

        return result;
    }

    private void Find2(AstNode node, EntityHandle entityHandleToSearchFor, IList<UsageAsTextLocation> found)
    {
        if (node.Role == Roles.BaseType)
        {
            var symbol = node.GetSymbol();

            var entity = symbol as IEntity;
            if (entity != null)
            {
                if (entity.MetadataToken == entityHandleToSearchFor)
                {
                    var usage = new UsageAsTextLocation()
                    {
                        TypeEntityHandle = entityHandleToSearchFor,
                        StartLocation = node.StartLocation,
                        EndLocation = node.EndLocation
                    };
                    found.Add(usage);
                }
            }
        }

        foreach (var child in node.Children)
        {
            Find2(child, entityHandleToSearchFor, found);
        }
    }
}
