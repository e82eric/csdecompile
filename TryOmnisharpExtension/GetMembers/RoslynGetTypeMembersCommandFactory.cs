using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using OmniSharp;
using TryOmnisharpExtension.FindUsages;

namespace TryOmnisharpExtension.GetMembers;

[Export]
public class RoslynGetTypeMembersCommandFactory
{
    private readonly OmniSharpWorkspace _workspace;

    [ImportingConstructor]
    public RoslynGetTypeMembersCommandFactory(OmniSharpWorkspace workspace)
    {
        _workspace = workspace;
    }
    
    public async Task<INavigationCommand<GetTypeMembersResponse>> Get(LocationRequest request)
    {
        var document = _workspace.GetDocument(request.FileName);
        var projectOutputFilePath = document.Project.OutputFilePath;
        var assemblyFilePath = projectOutputFilePath;
        var symbol = await GetDefinitionSymbol(document, request.Line, request.Column);

        INamedTypeSymbol namedTypeSymbol;
        if (symbol is INamedTypeSymbol)
        {
            namedTypeSymbol = symbol as INamedTypeSymbol;
        }
        else
        {
            namedTypeSymbol = symbol.ContainingType;
        }

        var command = new RoslynGetTypeMembersCommand(namedTypeSymbol, _workspace);
        return command;
    }

    internal async Task<ISymbol> GetDefinitionSymbol(Document document, int line, int column)
    {
        var sourceText = await document.GetTextAsync(CancellationToken.None);
        var position = GetPositionFromLineAndOffset(sourceText, line -1, column);
        var semanticInfo = await document.GetSemanticModelAsync();

        var enclosingSymbol = semanticInfo.GetEnclosingSymbol(position);
        var temp = semanticInfo.SyntaxTree.GetLocation(new TextSpan(line, column));
        var symbol = await SymbolFinder.FindSymbolAtPositionAsync(document, position, CancellationToken.None);
        return enclosingSymbol;


        return symbol switch
        {
            INamespaceSymbol => null,
            // Always prefer the partial implementation over the definition
            IMethodSymbol { IsPartialDefinition: true, PartialImplementationPart: var impl } => impl,
            // Don't return property getters/settings/initers
            IMethodSymbol { AssociatedSymbol: IPropertySymbol } => null,
            _ => symbol
        };
    }
    public static int GetPositionFromLineAndOffset(SourceText text, int lineNumber, int offset)
        => text.Lines[lineNumber].Start + offset;
}