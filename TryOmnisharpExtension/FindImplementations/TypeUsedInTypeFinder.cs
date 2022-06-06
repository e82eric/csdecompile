using System;
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
    
    public async Task<IEnumerable<UsageAsTextLocation>> Find2(ITypeDefinition rootTypeSymbol, EntityHandle typeEntityHandle)
    {
        var assemblyFilePath = rootTypeSymbol.ParentModule.PEFile.FileName;
        var decompiler = await _decompilerFactory.Get(assemblyFilePath);
        var (syntaxTree, source) = decompiler.Run(rootTypeSymbol);

        var result = new List<UsageAsTextLocation>();
        Find2(syntaxTree, typeEntityHandle, result);

        var lines = source.Split(new []{"\r\n"}, StringSplitOptions.None);
        foreach (var usageAsTextLocation in result)
        {
            var line = lines[usageAsTextLocation.StartLocation.Line - 1].Trim();
            usageAsTextLocation.Statement = line;
        }

        return result;
    }
    
    public async Task<UsageAsTextLocation> FindType(
        string typeNamespace,
        string typeName,
        SyntaxTree syntaxTree)
    {
        var namespaceNode = FindNamespace(syntaxTree, typeNamespace);
        var typeNode = FindType(namespaceNode, typeName);
        var typeIdentifier = FindTypeIdentifier(typeNode, typeName);

        var result = new UsageAsTextLocation
        {
            StartLocation = typeIdentifier.StartLocation,
            EndLocation = typeIdentifier.EndLocation
        };

        return result;
    }
    
    public async Task<UsageAsTextLocation> FindType(ITypeDefinition rootTypeSymbol, string typeNamespace, string typeName, string baseTypeName)
    {
        var assemblyFilePath = rootTypeSymbol.ParentModule.PEFile.FileName;
        var decompiler = await _decompilerFactory.Get(assemblyFilePath);
        var (syntaxTree, source) = decompiler.Run(rootTypeSymbol);

        var namespaceNode = FindNamespace(syntaxTree, typeNamespace);
        var typeNode = FindType(namespaceNode, typeName);
        var baseTypeNode = FindBaseType(typeNode, baseTypeName);

        var result = new UsageAsTextLocation
        {
            StartLocation = baseTypeNode.StartLocation,
            EndLocation = baseTypeNode.EndLocation
        };

        return result;
    }
    
    public async Task<UsageAsTextLocation> FindMethod(ITypeDefinition rootTypeSymbol, string typeNamespace, string typeName, string methodName)
    {
        var assemblyFilePath = rootTypeSymbol.ParentModule.PEFile.FileName;
        var decompiler = await _decompilerFactory.Get(assemblyFilePath);
        var (syntaxTree, source) = decompiler.Run(rootTypeSymbol);

        var namespaceNode = FindNamespace(syntaxTree, typeNamespace);
        var typeNode = FindType(namespaceNode, typeName);
        var baseTypeNode = FindMethod(typeNode, methodName);

        var result = new UsageAsTextLocation
        {
            StartLocation = baseTypeNode.StartLocation,
            EndLocation = baseTypeNode.EndLocation
        };

        return result;
    }
    
    public async Task<UsageAsTextLocation> FindProperty(ITypeDefinition rootTypeSymbol, string typeNamespace, string typeName, string methodName)
    {
        var assemblyFilePath = rootTypeSymbol.ParentModule.PEFile.FileName;
        var decompiler = await _decompilerFactory.Get(assemblyFilePath);
        var (syntaxTree, source) = decompiler.Run(rootTypeSymbol);

        var namespaceNode = FindNamespace(syntaxTree, typeNamespace);
        var typeNode = FindType(namespaceNode, typeName);
        var baseTypeNode = FindProperty(typeNode, methodName);

        var result = new UsageAsTextLocation
        {
            StartLocation = baseTypeNode.StartLocation,
            EndLocation = baseTypeNode.EndLocation
        };

        return result;
    }
    
    public async Task<UsageAsTextLocation> FindEvent(ITypeDefinition rootTypeSymbol, string typeNamespace, string typeName, string methodName)
    {
        var assemblyFilePath = rootTypeSymbol.ParentModule.PEFile.FileName;
        var decompiler = await _decompilerFactory.Get(assemblyFilePath);
        var (syntaxTree, source) = decompiler.Run(rootTypeSymbol);

        var namespaceNode = FindNamespace(syntaxTree, typeNamespace);
        var typeNode = FindType(namespaceNode, typeName);
        var baseTypeNode = FindEvent(typeNode, methodName);

        var result = new UsageAsTextLocation
        {
            StartLocation = baseTypeNode.StartLocation,
            EndLocation = baseTypeNode.EndLocation
        };

        return result;
    }
    
    private AstNode FindType(AstNode node, string typeName)
    {
        foreach (var child in node.Children)
        {
            if (child is TypeDeclaration)
            {
                var namespaceNode = (TypeDeclaration)child;
                if (namespaceNode.Name == typeName)
                {
                    return child;
                }
            }
    
            var result = FindType(child, typeName);
            if (result != null)
            {
                return result;
            }
        }
    
        return null;
    }
    
    private AstNode FindTypeIdentifier(AstNode typeNode, string typeName)
    {
        foreach (var child in typeNode.Children)
        {
            if (child is Identifier identifierNode)
            {
                if (identifierNode.Name == typeName)
                {
                    return child;
                }
            }
    
            var result = FindTypeIdentifier(child, typeName);
            if (result != null)
            {
                return result;
            }
        }
    
        return null;
    }
    
    private SimpleType FindBaseType(AstNode node, string baseTypeName)
    {
        foreach (var child in node.Children)
        {
            if (child is SimpleType)
            {
                var simpleTypeNode = (SimpleType)child;
                if (simpleTypeNode.Identifier == baseTypeName)
                {
                    return simpleTypeNode;
                }
            }
    
            var result = FindBaseType(child, baseTypeName);
            if (result != null)
            {
                return result;
            }
        }
    
        return null;
    }
    
    private MethodDeclaration FindMethod(AstNode node, string methodName)
    {
        foreach (var child in node.Children)
        {
            MethodDeclaration result;
            if (child is MethodDeclaration)
            {
                result = (MethodDeclaration)child;
                if (result.Name == methodName)
                {
                    return result;
                }
            }
    
            result = FindMethod(child, methodName);
            if (result != null)
            {
                return result;
            }
        }
    
        return null;
    }
    
    private PropertyDeclaration FindProperty(AstNode node, string methodName)
    {
        foreach (var child in node.Children)
        {
            PropertyDeclaration result;
            if (child is PropertyDeclaration)
            {
                result = (PropertyDeclaration)child;
                if (result.Name == methodName)
                {
                    return result;
                }
            }
    
            result = FindProperty(child, methodName);
            if (result != null)
            {
                return result;
            }
        }
    
        return null;
    }
    
    private EventDeclaration FindEvent(AstNode node, string methodName)
    {
        foreach (var child in node.Children)
        {
            EventDeclaration result;
            if (child is EventDeclaration)
            {
                result = (EventDeclaration)child;
                if (result.Name == methodName)
                {
                    return result;
                }
            }
    
            result = FindEvent(child, methodName);
            if (result != null)
            {
                return result;
            }
        }
    
        return null;
    }
    
    private AstNode FindNamespace(AstNode node, string typeNamespace)
    {
        foreach (var child in node.Children)
        {
            if (child is NamespaceDeclaration)
            {
                var namespaceNode = (NamespaceDeclaration)child;
                if (namespaceNode.Name == typeNamespace)
                {
                    return child;
                }
            }

            var result = FindNamespace(child, typeNamespace);
            if (result != null)
            {
                return result;
            }
        }

        return null;
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
