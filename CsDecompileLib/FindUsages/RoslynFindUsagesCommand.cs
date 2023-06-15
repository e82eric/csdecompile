using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using CsDecompileLib.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace CsDecompileLib.FindUsages;

public class RoslynFindUsagesCommand : INavigationCommand<FindImplementationsResponse>
{
    private readonly ISymbol _symbol;
    private readonly ICsDecompileWorkspace _workspace;

    public RoslynFindUsagesCommand(
        ISymbol symbol,
        ICsDecompileWorkspace workspace)
    {
        _symbol = symbol;
        _workspace = workspace;
    }

    public async Task<ResponsePacket<FindImplementationsResponse>> Execute()
    {
        var definition = await SymbolFinder.FindSourceDefinitionAsync(_symbol, _workspace.CurrentSolution);
        IEnumerable<ReferencedSymbol> usages =
            await SymbolFinder.FindReferencesAsync(definition ?? _symbol, _workspace.CurrentSolution);

        var body = new FindImplementationsResponse();
        foreach (var usage in usages)
        {
            foreach (var location in usage.Locations)
            {
                var sourceFileInfo = location.Location.GetSourceLineInfo(_workspace);
                await FillContainingTypeNames(location, sourceFileInfo);
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

    private async Task FillContainingTypeNames(ReferenceLocation location, SourceFileInfo toFill)
    {
        var lineSpan = location.Location.GetLineSpan();
        var enclosingSymbol = await GetDefinitionSymbol(location.Document, lineSpan.StartLinePosition.Line,
            lineSpan.StartLinePosition.Character);

        if (enclosingSymbol.ContainingType != null)
        {
            toFill.ContainingTypeShortName = enclosingSymbol.ContainingType.Name;
            toFill.ContainingTypeFullName = enclosingSymbol.ContainingType.MetadataName;
        }
        else
        {
            var fallbackParentClassSymbol = await GetParentTypeSymbol(location.Document, location.Location);
            if (fallbackParentClassSymbol != null)
            {
                toFill.ContainingTypeShortName = fallbackParentClassSymbol.Name;
                toFill.ContainingTypeFullName = fallbackParentClassSymbol.MetadataName;
            }
            else
            {
                toFill.ContainingTypeShortName = enclosingSymbol.Name;
                toFill.ContainingTypeFullName = enclosingSymbol.MetadataName;
            }
        }
    }

    public async Task<INamedTypeSymbol> GetParentTypeSymbol(Document document, Location location)
    {
        var syntaxRoot = await document.GetSyntaxRootAsync();
        var node = syntaxRoot.FindNode(location.SourceSpan);

        var symanticModel = await document.GetSemanticModelAsync();
        SyntaxNode classDeclarationNode = null;
        while (node.Parent != null)
        {
            if (node.IsKind(SyntaxKind.ClassDeclaration))
            {
                classDeclarationNode = node;
                break;
            }

            node = node.Parent;
        }

        if (classDeclarationNode != null)
        {
            var symbol = symanticModel.GetDeclaredSymbol(classDeclarationNode);
            if (symbol is INamedTypeSymbol namedTypeSymbol)
            {
                return namedTypeSymbol;
            }
        }

        return null;
    }

    internal async Task<ISymbol> GetDefinitionSymbol(Document document, int line, int column)
    {
        var sourceText = await document.GetTextAsync(CancellationToken.None);
        var position = GetPositionFromLineAndOffset(sourceText, line - 1, column);
        var symanticModel = await document.GetSemanticModelAsync();
        var enclosingSymbol = symanticModel.GetEnclosingSymbol(position);
        return enclosingSymbol;
    }

    private static int GetPositionFromLineAndOffset(SourceText text, int lineNumber, int offset)
        => text.Lines[lineNumber].Start + offset;
}