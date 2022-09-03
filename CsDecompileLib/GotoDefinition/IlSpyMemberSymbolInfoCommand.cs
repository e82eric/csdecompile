using System.Threading.Tasks;
using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GotoDefinition;

public class IlSpyMemberSymbolInfoCommand : INavigationCommand<SymbolInfo>
{
    private readonly IMember _symbol;

    public IlSpyMemberSymbolInfoCommand(IMember symbol)
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

        var response = new ResponsePacket<SymbolInfo>
        {
            Body = result,
            Success = true
        };
        
        return Task.FromResult(response);
    }
}