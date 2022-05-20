using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using OmniSharp;

namespace TryOmnisharpExtension.FindUsages;

public class RoslynFindUsagesCommand : INavigationCommand<FindUsagesResponse>
{
    private readonly ISymbol _symbol;
    private readonly OmniSharpWorkspace _workspace;

    public RoslynFindUsagesCommand(
        ISymbol symbol,
        OmniSharpWorkspace workspace)
    {
        _symbol = symbol;
        _workspace = workspace;
    }
    public async Task<FindUsagesResponse> Execute()
    {
        var definition = await SymbolFinder.FindSourceDefinitionAsync(_symbol, _workspace.CurrentSolution);
        var usages = await SymbolFinder.FindReferencesAsync(definition ?? _symbol, _workspace.CurrentSolution);
        var locations = usages.SelectMany(u => u.Locations).Select(l => l.Location).ToList();

        // always skip get/set methods of properties from the list of definition locations.
        var definitionLocations = usages.Select(u => u.Definition)
            .Where(def => !(def is IMethodSymbol method && method.AssociatedSymbol is IPropertySymbol))
            .SelectMany(def => def.Locations)
            .Where(loc => loc.IsInSource);

        locations.AddRange(definitionLocations);

        var result = new FindUsagesResponse();
        foreach (var location in locations)
        {
            var sourceFileInfo = location.GetSourceLineInfo(_workspace);
            result.Implementations.Add(sourceFileInfo);
        }

        return result;
    }
}