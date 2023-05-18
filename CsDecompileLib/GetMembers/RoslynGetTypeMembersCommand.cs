using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using Microsoft.CodeAnalysis;
using CsDecompileLib.Roslyn;

namespace CsDecompileLib.GetMembers;

public class RoslynGetTypeMembersCommand : INavigationCommand<FindImplementationsResponse>
{
    private readonly INamedTypeSymbol _symbol;
    private readonly ICsDecompileWorkspace _workspace;

    public RoslynGetTypeMembersCommand(
        INamedTypeSymbol symbol,
        ICsDecompileWorkspace workspace)
    {
        _symbol = symbol;
        _workspace = workspace;
    }
    public Task<ResponsePacket<FindImplementationsResponse>> Execute()
    {
        var members = _symbol.GetMembers();

        var body = new FindImplementationsResponse();
        foreach (var member in members)
        {
            if (!member.IsImplicitlyDeclared
                && !(member is IMethodSymbol methodSymbol && methodSymbol.AssociatedSymbol is IPropertySymbol))
            {
                foreach (var location in member.Locations)
                {
                    var sourceFileInfo = location.GetSourceLineInfo(_workspace);
                    sourceFileInfo.ContainingTypeShortName = _symbol.Name;
                    body.Implementations.Add(sourceFileInfo);
                }
            }
        }

        var result = ResponsePacket.Ok(body);

        return Task.FromResult(result);
    }
}