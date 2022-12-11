using System.Threading.Tasks;
using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler.IL;

namespace CsDecompileLib.GetSymbolInfo;

public class IlSpyVariableSymbolInfoCommand : IlSpySymbolInfoCommandBase, INavigationCommand<SymbolInfo>
{
    private readonly ILVariable _symbol;

    public IlSpyVariableSymbolInfoCommand(ILVariable symbol)
    {
        _symbol = symbol;
    }
        
    public Task<ResponsePacket<SymbolInfo>> Execute()
    {
        var result = new SymbolInfo();
        // AddIlSpyEntityCommonHeaderProperties(result, _symbol);
        result.Properties.Add("ShortName", _symbol.Name);
        result.Properties.Add("Type", _symbol.Type.ReflectionName);

        result.DisplayName = _symbol.Name;
        result.Kind = _symbol.Type.ReflectionName;

        var response = new ResponsePacket<SymbolInfo>
        {
            Body = result,
            Success = true
        };
        
        return Task.FromResult(response);
    }
}