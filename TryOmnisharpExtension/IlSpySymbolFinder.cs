using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using OmniSharp.Extensions;
using TryOmnisharpExtension.IlSpy;
using ISymbol = ICSharpCode.Decompiler.TypeSystem.ISymbol;
using SyntaxTree = ICSharpCode.Decompiler.CSharp.Syntax.SyntaxTree;

namespace TryOmnisharpExtension;

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
    
    public async Task<ITypeDefinition> FindTypeDefinition(string projectAssemblyFilePath, string symbolFullName)
    {
        var typeSystem = await _typeSystemFactory.GetTypeSystem(projectAssemblyFilePath);
        var tempFile = typeSystem.FindType(new FullTypeName(symbolFullName)) as ITypeDefinition;

        if (tempFile == null)
        {
            var tempFile2 = typeSystem.GetAllTypeDefinitions()
                .Where(d => d.FullName == symbolFullName).FirstOrDefault();

            tempFile = tempFile2 as ITypeDefinition;
        }

        return tempFile;
    }
    
    public async Task<ITypeDefinition> FindTypeDefinition(string symbolFullName, DecompilerTypeSystem typeSystem)
    {
        var tempFile = typeSystem.FindType(new FullTypeName(symbolFullName)) as ITypeDefinition;

        if (tempFile == null)
        {
            var tempFile2 = typeSystem
                .GetAllTypeDefinitions()
                .FirstOrDefault(d => d.FullName == symbolFullName);

            tempFile = tempFile2;
        }

        return tempFile;
    }
    
    public async Task<IMethod> FindMethod(string projectAssemblyFilePath, IMethodSymbol roslynMethod)
    {
        var typeFullName = roslynMethod.ContainingType.GetSymbolName();
        var typeDefinitionSymbol = await FindTypeDefinition(projectAssemblyFilePath, typeFullName);
        var method = FindMethod(typeDefinitionSymbol, roslynMethod);
        return method;
    }
    
    public async Task<IProperty> FindProperty(string projectAssemblyFilePath, string typeName, string propertyName)
    {
        var typeDefinitionSymbol = await FindTypeDefinition(projectAssemblyFilePath, typeName);
        var property = FindProperty(typeDefinitionSymbol, propertyName);
        return property;
    }
    
    public async Task<IEvent> FindEvent(string projectAssemblyFilePath, string typeName, string eventName)
    {
        var typeDefinitionSymbol = await FindTypeDefinition(projectAssemblyFilePath, typeName);
        var eventSymbol = FindEvent(typeDefinitionSymbol, eventName);
        return eventSymbol;
    }

    public async Task<ISymbol> FindSymbolAtLocation(string projectAssemblyFilePath, string containingTypeFullName, int line, int column)
    {
        var tempFile = await FindTypeDefinition(projectAssemblyFilePath, containingTypeFullName);
        var decompiler = await _decompilerFactory.Get(tempFile.ParentModule.PEFile.FileName);
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