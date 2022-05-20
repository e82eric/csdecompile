using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace TryOmnisharpExtension;

public class RosylynGotoDefinitionCommand : IGotoDefinitionCommand
{
    private readonly ISymbol _symbol;

    public RosylynGotoDefinitionCommand(
        ISymbol symbol)
    {
        _symbol = symbol;
    }
        
    public Task<DecompileGotoDefinitionResponse> Execute()
    {
        var lineSpan = _symbol.Locations.First().GetMappedLineSpan();
        var result = new DecompileGotoDefinitionResponse()
        { 
            Location = new SourceFileInfo
            {
                FileName = lineSpan.Path,
                Column = lineSpan.StartLinePosition.Character,
                Line = lineSpan.StartLinePosition.Line,
            },
            IsDecompiled = false,
        };
        return Task.FromResult(result);
    }
}