using System.Threading.Tasks;
using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GetSymbolInfo;

public class IlSpyParameterSymbolInfoCommand : IlSpySymbolInfoCommandBase, INavigationCommand<SymbolInfo>
{
    private readonly IParameter _symbol;

    public IlSpyParameterSymbolInfoCommand(IParameter symbol)
    {
        _symbol = symbol;
    }
        
    public Task<ResponsePacket<SymbolInfo>> Execute()
    {
        var result = new SymbolInfo();
        AddIlSpyEntityCommonHeaderProperties(result, _symbol);

        var response = new ResponsePacket<SymbolInfo>
        {
            Body = result,
            Success = true
        };
        
        return Task.FromResult(response);
    }
}