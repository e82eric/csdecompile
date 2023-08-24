using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CsDecompileLib.GetMembers;

public class RoslynGetTypeMembersCommandFactory
    : INavigationCommandFactoryAsync<INavigationCommand<LocationsResponse>, DecompiledLocationRequest>
{
    private readonly ICsDecompileWorkspace _workspace;

    public RoslynGetTypeMembersCommandFactory(ICsDecompileWorkspace workspace)
    {
        _workspace = workspace;
    }

    private SyntaxNode FindFirstClassNode(SyntaxNode node)
    {
        foreach (var childNode in node.ChildNodes())
        {
            if (childNode.IsKind(SyntaxKind.ClassDeclaration))
            {
                return childNode;
            }

            FindFirstClassNode(childNode);
        }

        return null;
    }
    
    public async Task<INavigationCommand<LocationsResponse>> Get(DecompiledLocationRequest request)
    {
        var document = _workspace.GetDocument(request.FileName);
        var semanticInfo = await document.GetSemanticModelAsync();
        var symbol = await GetDefinitionSymbol(document, semanticInfo, request.Line, request.Column);

        INamedTypeSymbol namedTypeSymbol;
        if (symbol is INamedTypeSymbol)
        {
            namedTypeSymbol = symbol as INamedTypeSymbol;
        }
        else
        {
            namedTypeSymbol = symbol.ContainingType;
        }

        if (namedTypeSymbol is null)
        {
            var rootAst = await semanticInfo.SyntaxTree.GetRootAsync();
            var classNodes = rootAst.DescendantNodes().OfType<TypeDeclarationSyntax>();

            var firstClassNode = classNodes.FirstOrDefault();

            if (firstClassNode != null)
            {
                namedTypeSymbol = semanticInfo.GetDeclaredSymbol(firstClassNode);
            }
        }

        var command = new RoslynGetTypeMembersCommand(namedTypeSymbol, _workspace);
        return command;
    }

    private async Task<ISymbol> GetDefinitionSymbol(Document document, SemanticModel semanticModel, int line, int column)
    {
        var sourceText = await document.GetTextAsync(CancellationToken.None);
        var position = GetPositionFromLineAndOffset(sourceText, line -1, column);

        var enclosingSymbol = semanticModel.GetEnclosingSymbol(position);
        return enclosingSymbol;
    }

    private static int GetPositionFromLineAndOffset(SourceText text, int lineNumber, int offset)
        => text.Lines[lineNumber].Start + offset;
}