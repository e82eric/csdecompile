using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using CsDecompileLib.FindImplementations;
using CsDecompileLib.Roslyn;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace CsDecompileLib.FindUsages;

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
    public async Task<ResponsePacket<FindImplementationsResponse>> Execute()
    {
        var definition = await SymbolFinder.FindSourceDefinitionAsync(_symbol, _workspace.CurrentSolution);
        IEnumerable<ReferencedSymbol> usages = await SymbolFinder.FindReferencesAsync(definition ?? _symbol, _workspace.CurrentSolution);

        var body = new FindImplementationsResponse();
        foreach (var usage in usages)
        {
            foreach (var location in usage.Locations)
            {
                var shortName = await GetShortName(location);
                var sourceFileInfo = location.Location.GetSourceLineInfo(_workspace);
                sourceFileInfo.ContainingTypeShortName = shortName;
                body.Implementations.Add(sourceFileInfo);
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

                        string shortName = null;
                        if (usage.Definition.ContainingType != null)
                        {
                            shortName = usage.Definition.ContainingType.Name;
                        }
                        else
                        {
                            shortName = usage.Definition.Name;
                        }

                        sourceFileInfo.ContainingTypeShortName = shortName;
                        body.Implementations.Add(sourceFileInfo);
                    }
                }
            }
        }

        var result = ResponsePacket.Ok(body);
        return result;
    }

    private async Task<string> GetShortName(ReferenceLocation location)
    {
        var lineSpan = location.Location.GetLineSpan();
        var enclosingSymbol = await GetDefinitionSymbol(location.Document, lineSpan.StartLinePosition.Line,
            lineSpan.StartLinePosition.Character);

        string shortName = null;
        if (enclosingSymbol.ContainingType != null)
        {
            shortName = enclosingSymbol.ContainingType.Name;
        }
        else
        {
            shortName = enclosingSymbol.Name;
        }

        return shortName;
    }

    internal async Task<ISymbol> GetDefinitionSymbol(Document document, int line, int column)
    {
        var sourceText = await document.GetTextAsync(CancellationToken.None);
        var position = GetPositionFromLineAndOffset(sourceText, line -1, column);
        var symanticModel = await document.GetSemanticModelAsync();
        var enclosingSymbol = symanticModel.GetEnclosingSymbol(position);
        return enclosingSymbol;
    }

    private static int GetPositionFromLineAndOffset(SourceText text, int lineNumber, int offset)
        => text.Lines[lineNumber].Start + offset;
}