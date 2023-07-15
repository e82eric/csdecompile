using System.Linq;
using System.Threading.Tasks;
using CsDecompileLib.Roslyn;
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

        string containingTypeFullName = null;
        if (_symbol is not INamedTypeSymbol)
        {
            containingTypeFullName = _symbol.ContainingType.GetMetadataName();
        }
        else
        {
            INamedTypeSymbol current = _symbol as INamedTypeSymbol;
            while (current.ContainingType != null)
            {
                current = current.ContainingType;
            }
            containingTypeFullName = current?.GetMetadataName();
        }

        var result = new GotoDefinitionResponse
        {
            Location = new SourceFileInfo
            {
                FileName = lineSpan.Path,
                Column = lineSpan.StartLinePosition.Character + 1,
                Line = lineSpan.StartLinePosition.Line + 1,
                ContainingTypeFullName = containingTypeFullName
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