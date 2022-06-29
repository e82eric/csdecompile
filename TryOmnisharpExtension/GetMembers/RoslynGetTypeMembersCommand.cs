using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using TryOmnisharpExtension.FindImplementations;
using TryOmnisharpExtension.IlSpy;
using TryOmnisharpExtension.Roslyn;

namespace TryOmnisharpExtension.GetMembers;

public class RoslynGetTypeMembersCommand : INavigationCommand<FindImplementationsResponse>
{
    private readonly INamedTypeSymbol _symbol;
    private readonly IOmniSharpWorkspace _workspace;

    public RoslynGetTypeMembersCommand(
        INamedTypeSymbol symbol,
        IOmniSharpWorkspace workspace)
    {
        _symbol = symbol;
        _workspace = workspace;
    }
    public Task<FindImplementationsResponse> Execute()
    {
        var members = _symbol.GetMembers();

        var result = new FindImplementationsResponse();
        foreach (var member in members)
        {
            if (!member.IsImplicitlyDeclared
                && !(member is IMethodSymbol methodSymbol && methodSymbol.AssociatedSymbol is IPropertySymbol))
            {
                foreach (var location in member.Locations)
                {
                    var sourceFileInfo = location.GetSourceLineInfo(_workspace);
                    result.Implementations.Add(sourceFileInfo);
                }
            }
        }

        return Task.FromResult(result);
    }
}