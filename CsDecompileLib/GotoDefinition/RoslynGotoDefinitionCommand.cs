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

        var sourceLineInfo = location.GetSourceLineInfo(_workspace);
        sourceLineInfo.ContainingTypeFullName = containingTypeFullName;

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