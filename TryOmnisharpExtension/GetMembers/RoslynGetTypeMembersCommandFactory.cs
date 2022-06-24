using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using OmniSharp;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.GetMembers;

[Export]
public class RoslynGetTypeMembersCommandFactory
{
    private readonly IOmniSharpWorkspace _workspace;

    [ImportingConstructor]
    public RoslynGetTypeMembersCommandFactory(IOmniSharpWorkspace workspace)
    {
        _workspace = workspace;
    }
    
    public async Task<INavigationCommand<GetTypeMembersResponse>> Get(LocationRequest request)
    {
        var document = _workspace.GetDocument(request.FileName);
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

    private async Task<ISymbol> GetDefinitionSymbol(Document document, int line, int column)
    {
        var sourceText = await document.GetTextAsync(CancellationToken.None);
        var position = GetPositionFromLineAndOffset(sourceText, line -1, column);
        var semanticInfo = await document.GetSemanticModelAsync();

        var enclosingSymbol = semanticInfo.GetEnclosingSymbol(position);
        return enclosingSymbol;
    }

    private static int GetPositionFromLineAndOffset(SourceText text, int lineNumber, int offset)
        => text.Lines[lineNumber].Start + offset;
}