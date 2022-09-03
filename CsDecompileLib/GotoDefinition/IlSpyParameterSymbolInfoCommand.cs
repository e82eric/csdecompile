using System.Threading.Tasks;
using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GotoDefinition;

public class IlSpyParameterSymbolInfoCommand : INavigationCommand<SymbolInfo>
{
    private readonly IParameter _symbol;

    public IlSpyParameterSymbolInfoCommand(IParameter symbol)
    {
        _symbol = symbol;
    }
        
    public Task<ResponsePacket<SymbolInfo>> Execute()
    {
        var result = new SymbolInfo();
        result.Properties.Add("ShortName", _symbol.Name);
        result.Properties.Add("Type", _symbol.Type.ReflectionName);

        var response = new ResponsePacket<SymbolInfo>
        {
            Body = result,
            Success = true
        };
        
        return Task.FromResult(response);
    }
}