using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace CsDecompileLib.GotoDefinition;

public class RoslynGotoDefinitionCommand : INavigationCommand<GotoDefinitionResponse>
{
    private readonly ISymbol _symbol;

    public RoslynGotoDefinitionCommand(ISymbol symbol)
    {
        _symbol = symbol;
    }
        
    public Task<ResponsePacket<GotoDefinitionResponse>> Execute()
    {
        var lineSpan = _symbol.Locations.First().GetMappedLineSpan();
        var result = new GotoDefinitionResponse
        { 
            Location = new SourceFileInfo
            {
                FileName = lineSpan.Path,
                Column = lineSpan.StartLinePosition.Character + 1,
                Line = lineSpan.StartLinePosition.Line + 1,
            },
        };

        var response = new ResponsePacket<GotoDefinitionResponse>()
        {
            Body = result,
            Success = true
        };
        
        return Task.FromResult(response);
    }
}