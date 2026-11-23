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

public class RoslynFindUsagesCommand : INavigationCommand<LocationsResponse>
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

    public async Task<ResponsePacket<LocationsResponse>> Execute()
    {
        var definition = await SymbolFinder.FindSourceDefinitionAsync(_symbol, _workspace.CurrentSolution);
        IEnumerable<ReferencedSymbol> usages =
            await SymbolFinder.FindReferencesAsync(definition ?? _symbol, _workspace.CurrentSolution);

        var body = new LocationsResponse();
        foreach (var usage in usages)
        {
            foreach (var location in usage.Locations)
            {
                var sourceFileInfo = location.Location.GetSourceLineInfo(_workspace);
                await FillContainingTypeNames(location, sourceFileInfo);
                body.Locations.Add(sourceFileInfo);
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
                        sourceFileInfo.ContainingTypeFullName = usage.Definition.ContainingType?.GetMetadataName();
                        body.Locations.Add(sourceFileInfo);
                    }
                }
            }
        }

        var result = ResponsePacket.Ok(body);
        return result;
    }

    private async Task FillContainingTypeNames(ReferenceLocation location, SourceFileInfo toFill)
    {
        var semanticModel = await location.Document.GetSemanticModelAsync();
        var enclosingSymbol = semanticModel.GetEnclosingSymbol(location.Location.SourceSpan.Start);

        //A usage outside of any member (a base list, an attribute on a type) is enclosed by the type itself
        var enclosingType = enclosingSymbol as INamedTypeSymbol ?? enclosingSymbol?.ContainingType;
        if (enclosingType != null)
        {
            toFill.ContainingTypeShortName = enclosingType.Name;
            toFill.ContainingTypeFullName = enclosingType.GetMetadataName();
        }
        else
        {
            var fallbackParentClassSymbol = await GetParentTypeSymbol(location.Document, location.Location);
            if (fallbackParentClassSymbol != null)
            {
                toFill.ContainingTypeShortName = fallbackParentClassSymbol.Name;
                toFill.ContainingTypeFullName = fallbackParentClassSymbol.GetMetadataName();
            }
            else if (enclosingSymbol != null)
            {
                toFill.ContainingTypeShortName = enclosingSymbol.Name;
                toFill.ContainingTypeFullName = enclosingSymbol.GetMetadataName();
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
}