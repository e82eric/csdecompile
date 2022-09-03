using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using SymbolInfo = TryOmnisharpExtension.Roslyn.SymbolInfo;

namespace TryOmnisharpExtension.GotoDefinition;

public class IlSpyMethodSymbolInfoCommand : INavigationCommand<SymbolInfo>
{
    private readonly IMethod _symbol;

    public IlSpyMethodSymbolInfoCommand(IMethod symbol)
    {
        _symbol = symbol;
    }
        
    public Task<ResponsePacket<SymbolInfo>> Execute()
    {
        var result = new SymbolInfo();
        result.Properties.Add("AssemblyPath", _symbol?.ParentModule.PEFile.FileName);
        result.Properties.Add("FullName", _symbol.ReflectionName);
        result.Properties.Add("ShortName", _symbol.Name);
        result.Properties.Add("Namespace", _symbol.Namespace);
        result.Properties.Add("Kind", _symbol.SymbolKind.ToString());
        result.Properties.Add("ReturnType", _symbol.ReturnType.ReflectionName);

        var parameters = new Dictionary<string, string>();
        foreach (var parameter in _symbol.Parameters)
        {
            parameters.Add(parameter.Name, parameter.Type.ReflectionName);
        }
        result.Properties.Add("Parameters", parameters);

        var overloads = GetOverloads();
        if (overloads.Any())
        {
            result.Properties.Add("Overloads", overloads);
        }

        var response = new ResponsePacket<SymbolInfo>
        {
            Body = result,
            Success = true
        };
        
        return Task.FromResult(response);
    }

    private IEnumerable<string> GetOverloads()
    {
        var result = new List<string>();
        foreach (var method in _symbol.DeclaringTypeDefinition.Methods)
        {
            if (method.Name == _symbol.Name && _symbol.Parameters.Count != method.Parameters.Count)
            {
                var parameters = GetParameters(method.Parameters);
                var parametersStr = string.Join(", ", parameters);
                var methodStr = $"{method.ReturnType.Name} {method.Name}({parametersStr})";
                result.Add(methodStr);
            }
        }

        return result;
    }

    private IEnumerable<string> GetParameters(IEnumerable<IParameter> parameters)
    {
        var result = new List<string>();

        foreach (var parameter in parameters)
        {
            result.Add($"{parameter.Type.Name} {parameter.Name}");
        }

        return result;
    }
}