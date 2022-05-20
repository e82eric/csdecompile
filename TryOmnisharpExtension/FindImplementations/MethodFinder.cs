using System.Composition;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class MethodFinder
{
    private readonly DecompilerFactory _decompilerFactory;

    [ImportingConstructor]
    public MethodFinder(DecompilerFactory decompilerFactory)
    {
        _decompilerFactory = decompilerFactory;
    }
    
    public async Task<UsageAsTextLocation> Find(IMethod symbol, EntityHandle typeEntityHandle, ITypeDefinition rootType)
    {
        var fileName = symbol.Compilation.MainModule.PEFile.FileName;
        var decompiler = await _decompilerFactory.Get(fileName);

        var (syntaxTree, sourceText) = decompiler.Run(rootType);

        var methodBodyNode = FindMethodBody(syntaxTree, symbol.MetadataToken);

        if (methodBodyNode == null)
        {
            return null;
        }

        var usage = new UsageAsTextLocation
        {
            TypeEntityHandle = typeEntityHandle,
            StartLocation = methodBodyNode.StartLocation,
            EndLocation = methodBodyNode.EndLocation,
            Statement = methodBodyNode.ToString()
        };

        return usage;
    }

    private AstNode FindMethodBody(AstNode node, EntityHandle entityHandle)
    {
        if (node.NodeType == NodeType.Member)
        {
            var symbol = node.GetSymbol();
            if (symbol is IEntity entity)
            {
                if (entity.MetadataToken == entityHandle)
                {
                    if (node is MethodDeclaration method)
                    {
                        return method;
                    }

                    return null;
                }
            }
        }

        if (node.HasChildren)
        {
            foreach (var child in node.Children)
            {
                var method = FindMethodBody(child, entityHandle);
                if (method != null)
                {
                    return method;
                }
            }
        }

        return null;
    }
}