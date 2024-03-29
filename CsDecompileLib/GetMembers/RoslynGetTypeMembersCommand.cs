﻿using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using Microsoft.CodeAnalysis;
using CsDecompileLib.Roslyn;

namespace CsDecompileLib.GetMembers;

public class RoslynGetTypeMembersCommand : INavigationCommand<LocationsResponse>
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
    public Task<ResponsePacket<LocationsResponse>> Execute()
    {
        var members = _symbol.GetMembers();

        var body = new LocationsResponse();
        foreach (var member in members)
        {
            if (!member.IsImplicitlyDeclared
                && !(member is IMethodSymbol methodSymbol && methodSymbol.AssociatedSymbol is IPropertySymbol))
            {
                foreach (var location in member.Locations)
                {
                    var sourceFileInfo = location.GetSourceLineInfo(_workspace);
                    sourceFileInfo.ContainingTypeShortName = _symbol.Name;
                    sourceFileInfo.ContainingTypeFullName = _symbol.GetMetadataName();
                    body.Locations.Add(sourceFileInfo);
                }
            }
        }

        var result = ResponsePacket.Ok(body);

        return Task.FromResult(result);
    }
}