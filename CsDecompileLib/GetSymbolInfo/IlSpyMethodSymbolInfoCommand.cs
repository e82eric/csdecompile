using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GetSymbolInfo;

public class IlSpyMethodSymbolInfoCommand : IlSpySymbolInfoCommandBase, INavigationCommand<Roslyn.SymbolInfo>
{
    private readonly IMethod _symbol;

    public IlSpyMethodSymbolInfoCommand(IMethod symbol)
    {
        _symbol = symbol;
    }
        
    public Task<ResponsePacket<Roslyn.SymbolInfo>> Execute()
    {
        var result = new Roslyn.SymbolInfo();
        AddIlSpyEntityCommonHeaderProperties(result, _symbol);
        result.Properties.Add("FullName", _symbol.ReflectionName);
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

        var response = new ResponsePacket<Roslyn.SymbolInfo>
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