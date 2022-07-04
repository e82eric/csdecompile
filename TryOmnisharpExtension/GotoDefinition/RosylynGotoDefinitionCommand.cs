using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace TryOmnisharpExtension.GotoDefinition;

public class RosylynGotoDefinitionCommand : INavigationCommand<DecompileGotoDefinitionResponse>
{
    private readonly ISymbol _symbol;

    public RosylynGotoDefinitionCommand(ISymbol symbol)
    {
        _symbol = symbol;
    }
        
    public Task<ResponsePacket<DecompileGotoDefinitionResponse>> Execute()
    {
        var lineSpan = _symbol.Locations.First().GetMappedLineSpan();
        var result = new DecompileGotoDefinitionResponse
        { 
            Location = new SourceFileInfo
            {
                FileName = lineSpan.Path,
                Column = lineSpan.StartLinePosition.Character + 1,
                Line = lineSpan.StartLinePosition.Line + 1,
            },
            IsDecompiled = false,
        };

        var response = new ResponsePacket<DecompileGotoDefinitionResponse>()
        {
            Body = result,
            Success = true
        };
        
        return Task.FromResult(response);
    }
}