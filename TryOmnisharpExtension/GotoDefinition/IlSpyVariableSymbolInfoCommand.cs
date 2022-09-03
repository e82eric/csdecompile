using System.Threading.Tasks;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.Roslyn;

namespace TryOmnisharpExtension.GotoDefinition;

public class IlSpyVariableSymbolInfoCommand : INavigationCommand<SymbolInfo>
{
    private readonly ILVariable _symbol;

    public IlSpyVariableSymbolInfoCommand(ILVariable symbol)
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