using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using TryOmnisharpExtension.FindImplementations;
using TryOmnisharpExtension.IlSpy;
using TryOmnisharpExtension.Roslyn;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace TryOmnisharpExtension.FindUsages;

public class RoslynFindUsagesCommand : INavigationCommand<FindImplementationsResponse>
{
    private readonly ISymbol _symbol;
    private readonly IOmniSharpWorkspace _workspace;

    public RoslynFindUsagesCommand(
        ISymbol symbol,
        IOmniSharpWorkspace workspace)
    {
        _symbol = symbol;
        _workspace = workspace;
    }
    public async Task<FindImplementationsResponse> Execute()
    {
        var definition = await SymbolFinder.FindSourceDefinitionAsync(_symbol, _workspace.CurrentSolution);
        var usages = await SymbolFinder.FindReferencesAsync(definition ?? _symbol, _workspace.CurrentSolution);

        var result = new FindImplementationsResponse();
        foreach (var usage in usages)
        {
            foreach (var location in usage.Locations)
            {
                var sourceFileInfo = location.Location.GetSourceLineInfo(_workspace);
                result.Implementations.Add(sourceFileInfo);
            }

            //IsImplicitlyDeclared gets rid of auto generated stuff
            //The associated symbol check gets rid of getters and setters
            if (!usage.Definition.IsImplicitlyDeclared &&
                !(usage.Definition is IMethodSymbol methodSymbol && methodSymbol.AssociatedSymbol is IPropertySymbol)
                )
            {
                foreach (var location in usage.Definition.Locations)
                {
                    //Note that this will return the location that was searched from and the definition of the symbol
                    if (location.IsInSource)
                    {
                        var sourceFileInfo = location.GetSourceLineInfo(_workspace);
                        result.Implementations.Add(sourceFileInfo);
                    }
                }
            }
        }

        return result;
    }
}