using System.Linq;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using CsDecompileLib.Roslyn;
using Microsoft.CodeAnalysis;

namespace CsDecompileLib.GotoDefinition;

public class RoslynGotoDefinitionCommand : INavigationCommand<LocationsResponse>
{
    private readonly ISymbol _symbol;
    private readonly ICsDecompileWorkspace _workspace;

    public RoslynGotoDefinitionCommand(ISymbol symbol, ICsDecompileWorkspace workspace)
    {
        _symbol = symbol;
        _workspace = workspace;
    }

    public Task<ResponsePacket<LocationsResponse>> Execute()
    {
        var location = _symbol.Locations.First();

        INamedTypeSymbol containingType;
        if (_symbol is not INamedTypeSymbol)
        {
            containingType = _symbol.ContainingType;
        }
        else
        {
            INamedTypeSymbol current = _symbol as INamedTypeSymbol;
            while (current.ContainingType != null)
            {
                current = current.ContainingType;
            }

            containingType = current;
        }

        var sourceLineInfo = location.GetSourceLineInfo(_workspace);
        sourceLineInfo.ContainingTypeShortName = containingType.Name;
        sourceLineInfo.ContainingTypeFullName = containingType.GetMetadataName();

        var result = new LocationsResponse
        {
            Locations =
            {
                sourceLineInfo,
            }
        };

        var response = new ResponsePacket<LocationsResponse>()
        {
            Body = result,
            Success = true
        };

        return Task.FromResult(response);
    }
}