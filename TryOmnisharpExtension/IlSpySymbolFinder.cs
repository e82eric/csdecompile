using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension;

[Export]
public class IlSpySymbolFinder
{
    private readonly IlSpyTypeSystemFactory _typeSystemFactory;
    private readonly DecompilerFactory _decompilerFactory;

    [ImportingConstructor]
    public IlSpySymbolFinder(
        IlSpyTypeSystemFactory typeSystemFactory,
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
    public async Task<IMethod> FindMethod(string projectAssemblyFilePath, string typeName, string methodName, IReadOnlyList<string> methodParameterTypes)
    {
        var typeDefinitionSymbol = await FindTypeDefinition(projectAssemblyFilePath, typeName);
        var method = FindMethod(typeDefinitionSymbol, methodName, methodParameterTypes);
        return method;
    }
    
    public async Task<IProperty> FindProperty(string projectAssemblyFilePath, string typeName, string propertyName)
    {
        var typeDefinitionSymbol = await FindTypeDefinition(projectAssemblyFilePath, typeName);
        var property = FindProperty(typeDefinitionSymbol, propertyName);
        return property;
    }

    public async Task<ISymbol> FindSymbolAtLocation(string projectAssemblyFilePath, string containingTypeFullName, int line, int column)
    {
        var tempFile = await FindTypeDefinition(projectAssemblyFilePath, containingTypeFullName);
        var decompiler = await _decompilerFactory.Get(tempFile.ParentModule.PEFile.FileName);
        (SyntaxTree syntaxTree, string source) = decompiler.Run(containingTypeFullName);
            
        var node = syntaxTree.GetNodeAt(line, column);
        var symbolAtLocation = node.GetSymbol();

        while (symbolAtLocation == null && node.Parent != null)
        {
            node = node.Parent;
            symbolAtLocation = node?.GetSymbol();
        }

        return symbolAtLocation;
    }
    
    private IMethod FindMethod(ITypeDefinition type, string methodName, IReadOnlyList<string> methodParameterTypes)
    {
        var methods = new List<IMember>();

        foreach (var member in type.Members)
        {
            if (member.FullName == methodName)
            {
                var method = member as IMethod;

                if (method != null)
                {
                    if (method.Parameters.Count == methodParameterTypes.Count)
                    {
                        var paramsMatch = true;
                        for (int i = 0; i < methodParameterTypes.Count; i++)
                        {
                            if (method.Parameters[i].Type.ReflectionName != methodParameterTypes[i])
                            {
                                paramsMatch = false;
                            }
                        }

                        if (paramsMatch)
                        {
                            methods.Add(member);
                        }
                    }
                }
            }
        }

        if (methods.Count > 1)
        {
            foreach (var method in methods)
            {
                if (method.DeclaringType?.Name == type.Name)
                {
                    return method as IMethod;
                }
            }
        }

        return methods.FirstOrDefault() as IMethod;
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

        return (IProperty)properties.FirstOrDefault();
    }
}