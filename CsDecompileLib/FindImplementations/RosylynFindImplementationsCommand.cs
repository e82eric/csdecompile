using System.Linq;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using CsDecompileLib.Roslyn;

namespace CsDecompileLib.FindImplementations;

public class RosylynFindImplementationsCommand : INavigationCommand<FindImplementationsResponse>
{
    private readonly ISymbol _symbol;
    private readonly IOmniSharpWorkspace _workspace;

    public RosylynFindImplementationsCommand(
        ISymbol symbol,
        IOmniSharpWorkspace workspace)
    {
        _symbol = symbol;
        _workspace = workspace;
    }
        
    public async Task<ResponsePacket<FindImplementationsResponse>> Execute()
    {
        var body = new FindImplementationsResponse();

        if (_symbol.IsSealed)
        {
            var noOp = ResponsePacket.Ok(body);
            return noOp;
        }

        if (_symbol.IsInterfaceType() || _symbol.IsImplementableMember())
        {
            // SymbolFinder.FindImplementationsAsync will not include the method overrides
            var implementations = await SymbolFinder.FindImplementationsAsync(_symbol, _workspace.CurrentSolution);
            foreach (var implementation in implementations)
            {
                if (!implementation.Locations.First().IsInSource)
                {
                    //Skip since this will get covered by IlSpy
                }
                else
                {
                    var sourceInfo = implementation.GetSourceLineInfo(_workspace);
                    body.Implementations.Add(sourceInfo);

                    if (implementation.IsOverridable())
                    {
                        var overrides = await SymbolFinder.FindOverridesAsync(implementation, _workspace.CurrentSolution);
                        foreach (var @override in overrides)
                        {
                            body.Implementations.Add(@override.GetSourceLineInfo(_workspace));
                        }
                    }
                }
            }
        }
        else if (_symbol is INamedTypeSymbol namedTypeSymbol)
        {
            // for types also include derived classes
            // for other symbols, find overrides and include those
            var derivedTypes = await SymbolFinder.FindDerivedClassesAsync(namedTypeSymbol, _workspace.CurrentSolution);
            foreach (var derivedType in derivedTypes)
            {
                if (derivedType.Locations.First().IsInSource)
                {
                    var sourceLineInfo = derivedType.GetSourceLineInfo(_workspace);
                    body.Implementations.Add(sourceLineInfo);
                }
            }
        }
        else if (_symbol.IsOverridable())
        {
            var overrides = await SymbolFinder.FindOverridesAsync(_symbol, _workspace.CurrentSolution);
            foreach (var @override in overrides)
            {
                if (@override.Locations.First().IsInSource)
                {
                    var sourceLineInfo = @override.GetSourceLineInfo(_workspace);
                    body.Implementations.Add(sourceLineInfo);
                }
            }
        }

        // also include the original declaration of the symbol
        if (!_symbol.IsAbstract && _symbol.Locations.First().IsInSource)
        {
            // for partial methods, pick the one with body
            if (_symbol is IMethodSymbol method && method.PartialImplementationPart != null)
            {
                var sourceLineInfo = method.PartialImplementationPart.GetSourceLineInfo(_workspace);
                body.Implementations.Add(sourceLineInfo);
            }
            else
            {
                var sourceLineInfo = _symbol.GetSourceLineInfo(_workspace);
                body.Implementations.Add(sourceLineInfo);
            }
        }

        var result = ResponsePacket.Ok(body);
        return result;
    }
}