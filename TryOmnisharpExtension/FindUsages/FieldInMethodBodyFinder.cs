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
public class FieldInMethodBodyFinder
{
    private readonly DecompilerFactory _decompilerFactory;

    [ImportingConstructor]
    public FieldInMethodBodyFinder(DecompilerFactory decompilerFactory)
    {
        _decompilerFactory = decompilerFactory;
    }
    public async Task<IEnumerable<UsageAsTextLocation>> Find(IMember symbol, ITypeDefinition rootType, IField field)
    {
        var fileName = symbol.Compilation.MainModule.PEFile.FileName;
        var cachingDecompiler = await _decompilerFactory.Get(fileName);
        var result = new List<UsageAsTextLocation>();

        var (syntaxTree, source) = cachingDecompiler.Run(rootType);

        if (symbol is IProperty propertyDeclaration)
        {
            var declarationNode = FindPropertyDeclaration(syntaxTree, symbol.MetadataToken);
            var getterBody = declarationNode?.Getter?.Body;
            if (getterBody != null)
            {
                Find(getterBody, field.MetadataToken, result);
            }
            var setterBody = declarationNode?.Setter?.Body;
            if (setterBody != null)
            {
                Find(setterBody, field.MetadataToken, result);
            }
        }
        else
        {
            var methodBodyNode = FindMethodBody(syntaxTree, symbol.MetadataToken);
            if (methodBodyNode != null)
            {
                Find(methodBodyNode, field.MetadataToken, result);
            }
        }

        return result;
    }
    
    private PropertyDeclaration FindPropertyDeclaration(AstNode node, EntityHandle entityHandle)
    {
        if (node.NodeType == NodeType.Member)
        {
            var symbol = node.GetSymbol();
            if (symbol != null)
            {
                if (symbol is IEntity entity)
                {
                    if (entity.MetadataToken == entityHandle)
                    {
                        if (node is PropertyDeclaration propertyDeclaration)
                        {
                            return propertyDeclaration;
                        }
                    }
                }
            }
        }

        if (node.HasChildren)
        {
            foreach (var child in node.Children)
            {
                var result = FindPropertyDeclaration(child, entityHandle);
                if (result != null)
                {
                    return result;
                }
            }
        }

        return null;
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

    private void Find(AstNode node, EntityHandle fieldEntityHandle, IList<UsageAsTextLocation> found)
    {
        var symbol = node.GetSymbol();

        if (symbol != null)
        {
            var entity = symbol as IField;

            if(entity != null)
            {
                if (entity.MetadataToken == fieldEntityHandle)
                {
                    var usage = new UsageAsTextLocation()
                    {
                        TypeEntityHandle = fieldEntityHandle,
                        StartLocation = node.StartLocation,
                        EndLocation = node.EndLocation,
                        Statement = node.Parent.ToString()
                    };

                    found.Add(usage);
                }
            }
        }

        foreach (var child in node.Children)
        {
            Find(child, fieldEntityHandle, found);
        }
    }
}