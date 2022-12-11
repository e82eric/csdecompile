using System.Threading.Tasks;
using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GetSymbolInfo;

public class IlSpyMemberSymbolInfoCommand : IlSpySymbolInfoCommandBase, INavigationCommand<SymbolInfo>
{
    private readonly IMember _symbol;

    public IlSpyMemberSymbolInfoCommand(IMember symbol)
    {
        _symbol = symbol;
    }
        
    public Task<ResponsePacket<SymbolInfo>> Execute()
    {
        var result = new SymbolInfo();
        result.Properties.Add("FullName", _symbol.ReflectionName);
        result.Properties.Add("ReturnType", _symbol.ReturnType.ReflectionName);

        AddIlSpyEntityCommonHeaderProperties(result, _symbol);

        var response = new ResponsePacket<SymbolInfo>
        {
            Body = result,
            Success = true
        };
        
        return Task.FromResult(response);
    }
}