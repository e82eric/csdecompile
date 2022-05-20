using System.Collections.Generic;
using System.Composition;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages;

[Export]
public class PropertyInMethodBodyFinder
{
    private readonly DecompilerFactory _decompilerFactory;

    [ImportingConstructor]
    public PropertyInMethodBodyFinder(DecompilerFactory decompilerFactory)
    {
        _decompilerFactory = decompilerFactory;
    }
    public async Task<IEnumerable<UsageAsTextLocation>> Find(IMethod symbol, EntityHandle? setterEntityHandle, EntityHandle? getterEntityHandle, ITypeDefinition rootType)
    {
        var fileName = symbol.Compilation.MainModule.PEFile.FileName;
        var cachingDecompiler = await _decompilerFactory.Get(fileName);
        var result = new List<UsageAsTextLocation>();

        var (syntaxTree, source) = cachingDecompiler.Run(rootType);

        var methodBodyNode = FindMethodBody(syntaxTree, symbol.MetadataToken);
        if (methodBodyNode != null)
        {
            Find(methodBodyNode, setterEntityHandle, getterEntityHandle, rootType.MetadataToken, result);
        }

        return result;
    }

    private AstNode FindMethodBody(AstNode node, EntityHandle entityHandle)
    {
        if (node.NodeType == NodeType.Member)
        {
            var symbol = node.GetSymbol();
            var entity = symbol as IEntity;
            if (entity != null)
            {
                if (entity.MetadataToken == entityHandle)
                {
                    var method = node as MethodDeclaration;

                    if (method != null)
                    {
                        return method.Body;
                    }

                    var constructor = node as ConstructorDeclaration;

                    if (constructor != null)
                    {
                        return constructor.Body;
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

    private void Find(AstNode node, EntityHandle? setterEntityHandle, EntityHandle? getterEntityHandle, EntityHandle rootTypeEntityHandle, IList<UsageAsTextLocation> found)
    {
        var symbol = node.GetSymbol();

        if (symbol != null)
        {
            var entity = symbol as IProperty;

            if(entity != null)
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

                        found.Add(usage);
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

                        found.Add(usage);
                    }
                }
            }
        }

        foreach (var child in node.Children)
        {
            Find(child, setterEntityHandle, getterEntityHandle, rootTypeEntityHandle, found);
        }
    }
}