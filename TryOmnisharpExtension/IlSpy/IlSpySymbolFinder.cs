using System.Composition;
using System.Linq;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using OmniSharp.Extensions;
using ISymbol = ICSharpCode.Decompiler.TypeSystem.ISymbol;
using SyntaxTree = ICSharpCode.Decompiler.CSharp.Syntax.SyntaxTree;

namespace TryOmnisharpExtension.IlSpy;

[Export]
public class IlSpySymbolFinder
{
    private readonly IDecompilerTypeSystemFactory _typeSystemFactory;
    private readonly DecompilerFactory _decompilerFactory;

    [ImportingConstructor]
    public IlSpySymbolFinder(
        IDecompilerTypeSystemFactory typeSystemFactory,
        DecompilerFactory decompilerFactory)
    {
        _typeSystemFactory = typeSystemFactory;
        _decompilerFactory = decompilerFactory;
    }
    
    public ITypeDefinition FindTypeDefinition(string projectAssemblyFilePath, string symbolFullName)
    {
        var typeSystem = _typeSystemFactory.GetTypeSystem(projectAssemblyFilePath);
        if (typeSystem == null)
        {
            return null;
        }
        var tempFile = typeSystem.FindType(new FullTypeName(symbolFullName)) as ITypeDefinition;

        if (tempFile == null)
        {
            var tempFile2 = typeSystem.GetAllTypeDefinitions()
                .Where(d => d.FullName == symbolFullName).FirstOrDefault();

            tempFile = tempFile2 as ITypeDefinition;
        }

        return tempFile;
    }
    
    public IMethod FindMethod(string projectAssemblyFilePath, IMethodSymbol roslynMethod)
    {
        var typeFullName = roslynMethod.ContainingType.GetSymbolName();
        var typeDefinitionSymbol = FindTypeDefinition(projectAssemblyFilePath, typeFullName);
        var method = FindMethod(typeDefinitionSymbol, roslynMethod);
        return method;
    }
    
    public IField FindField(string projectAssemblyFilePath, IFieldSymbol roslynMethod)
    {
        var typeFullName = roslynMethod.ContainingType.GetSymbolName();
        var typeDefinitionSymbol = FindTypeDefinition(projectAssemblyFilePath, typeFullName);
        var result = FindField(typeDefinitionSymbol, roslynMethod);
        return result;
    }
    
    public IProperty FindProperty(string projectAssemblyFilePath, string typeName, string propertyName)
    {
        var typeDefinitionSymbol = FindTypeDefinition(projectAssemblyFilePath, typeName);
        var property = FindProperty(typeDefinitionSymbol, propertyName);
        return property;
    }
    
    public IEvent FindEvent(string projectAssemblyFilePath, string typeName, string eventName)
    {
        var typeDefinitionSymbol = FindTypeDefinition(projectAssemblyFilePath, typeName);
        var eventSymbol = FindEvent(typeDefinitionSymbol, eventName);
        return eventSymbol;
    }
    
    public (AstNode, SyntaxTree, string) FindNode(ITypeDefinition typeDefinition, int line, int column)
    {
        var decompiler = _decompilerFactory.Get(typeDefinition.ParentModule.PEFile.FileName);
        (SyntaxTree syntaxTree, string source) = decompiler.Run(typeDefinition);
            
        var node = GetNodeAt(syntaxTree, line, column);
        return (node, syntaxTree, source);
    }
    
    public ISymbol FindSymbolFromNode(AstNode node)
    {
        var symbolAtLocation = node.GetSymbol();

        while (symbolAtLocation == null && node.Parent != null)
        {
            node = node.Parent;
            symbolAtLocation = node?.GetSymbol();
        }

        return symbolAtLocation;
    }

    public ISymbol FindSymbolAtLocation(string projectAssemblyFilePath, string containingTypeFullName, int line, int column)
    {
        var tempFile = FindTypeDefinition(projectAssemblyFilePath, containingTypeFullName);
        var decompiler = _decompilerFactory.Get(tempFile.ParentModule.PEFile.FileName);
        (SyntaxTree syntaxTree, string source) = decompiler.Run(containingTypeFullName);
            
        var node = GetNodeAt(syntaxTree, line, column);
        var symbolAtLocation = node.GetSymbol();

        while (symbolAtLocation == null && node.Parent != null)
        {
            node = node.Parent;
            symbolAtLocation = node?.GetSymbol();
        }

        return symbolAtLocation;
    }

    private AstNode GetNodeAt(AstNode node, int startLine, int startColumn)
    {
        foreach (var child in node.Children)
        {
            if (child.StartLocation.Line == startLine && child.StartLocation.Column == startColumn)
            {
                //We need to keep going down the tree to see if there is a more specific symbol, this could be an expression and we want the specific symbol
                //Ideally start sending end location line and column
                //This is inefficent we can stop recursing the rest of the tree here.  Just haven't figured that out yet
                if (!child.Children.Any(c =>
                        c.StartLocation.Line == startLine && c.StartLocation.Column == startColumn))
                {
                    return child;
                }
            }

            if (child.HasChildren)
            {
                var result = GetNodeAt(child, startLine, startColumn);
                if (result != null)
                {
                    return result;
                }
            }
        }
        return null;
    }
    
    private IMethod FindMethod(ITypeDefinition type, IMethodSymbol roslynMethod)
    {
        foreach (var member in type.Members)
        {
            if (member is IMethod ilSpyMethod)
            {
                if (RoslynToIlSpyEqualityExtensions.AreSameMethod(roslynMethod, ilSpyMethod))
                {
                    return ilSpyMethod;
                }

                //Fallback for checking extension methods.  Roslyn seems to add a higher level method symbol without
                //The this parameter
                if (roslynMethod.IsExtensionMethod)
                {
                    if (roslynMethod.ReducedFrom != null)
                    {
                        if (RoslynToIlSpyEqualityExtensions.AreSameMethod(roslynMethod.ReducedFrom, ilSpyMethod))
                        {
                            return ilSpyMethod;
                        }
                    }
                }
            }
        }

        return null;
    }
    
    private IField FindField(ITypeDefinition type, IFieldSymbol roslynField)
    {
        foreach (var member in type.Members)
        {
            if (member is IField ilSpySymbol)
            {
                if (RoslynToIlSpyEqualityExtensions.AreMemberSymbol(roslynField, ilSpySymbol))
                {
                    return ilSpySymbol;
                }
            }
        }

        return null;
    }
    
    private static IProperty FindProperty(IType type, string methodName)
    {
        var properties = type.GetProperties().Where(m =>
        {
            if (m.FullName != methodName)
            {
                return false;
            }

            return true;
        });

        return properties.FirstOrDefault();
    }
    
    private static IEvent FindEvent(IType type, string eventName)
    {
        var properties = type.GetEvents().Where(m =>
        {
            if (m.FullName != eventName)
            {
                return false;
            }

            return true;
        });

        return properties.FirstOrDefault();
    }
}