using ICSharpCode.Decompiler.CSharp.Syntax;

namespace TryOmnisharpExtension.IlSpy;

public static class AstExtensions
{
    public static AstNode FindTypeDeclaration(this AstNode node, string typeName)
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
    
            var result = child.FindTypeDeclaration(typeName);
            if (result != null)
            {
                return result;
            }
        }
    
        return null;
    }
    
    public static MethodDeclaration FindMethodDeclaration(this AstNode node, string methodName)
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
    
            result = child.FindMethodDeclaration(methodName);
            if (result != null)
            {
                return result;
            }
        }
    
        return null;
    }
}