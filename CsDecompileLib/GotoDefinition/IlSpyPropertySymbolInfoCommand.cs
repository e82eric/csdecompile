using System.Collections.Generic;
using System.Threading.Tasks;
using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GotoDefinition;

public class IlSpyPropertySymbolInfoCommand : INavigationCommand<SymbolInfo>
{
    private readonly IProperty _symbol;

    public IlSpyPropertySymbolInfoCommand(IProperty symbol)
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

        var response = new ResponsePacket<SymbolInfo>
        {
            Body = result,
            Success = true
        };
        
        return Task.FromResult(response);
    }
}