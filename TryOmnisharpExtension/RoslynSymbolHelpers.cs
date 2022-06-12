using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using OmniSharp;
using OmniSharp.Extensions;

namespace TryOmnisharpExtension;

internal static class RoslynSymbolHelpers
{
    public static SourceFileInfo GetSourceLineInfo(this Location location, OmniSharpWorkspace workspace)
    {
        var lineSpan = location.GetMappedLineSpan();

        var documents = workspace.GetDocuments(lineSpan.Path);
        var sourceText = GetSourceText(location, documents, lineSpan.HasMappedPath);
        var text = GetLineText(location, sourceText, lineSpan.StartLinePosition.Line).Trim();

        var sourceLineInfo = new SourceFileInfo
        {
            Line = lineSpan.StartLinePosition.Line + 1,
            Column = lineSpan.StartLinePosition.Character + 1,
            FileName = lineSpan.Path,
            SourceText = text.Trim()
        };

        return sourceLineInfo;

        static SourceText GetSourceText(Location location, IEnumerable<Document> documents, bool hasMappedPath)
        {
            // if we have a mapped linespan and we found a corresponding document, pick that one
            // otherwise use the SourceText of current location
            if (hasMappedPath)
            {
                SourceText source = null;
                if (documents != null && documents.FirstOrDefault(d => d != null && d.TryGetText(out source)) != null)
                {
                    // we have a mapped document that exists in workspace
                    return source;
                }

                // we have a mapped document that doesn't exist in workspace
                // in that case we have to always fall back to original linespan
                return null;
            }

            // unmapped document so just continue with current SourceText
            return location.SourceTree.GetText();
        }

        static string GetLineText(Location location, SourceText sourceText, int startLine)
        {
            // bounds check in case the mapping is incorrect, since user can put whatever line they want
            if (sourceText != null && sourceText.Lines.Count > startLine)
            {
                return sourceText.Lines[startLine].ToString();
            }

            // in case we fall out of bounds, we shouldn't crash, fallback to text from the unmapped span and the current file
            var fallBackLineSpan = location.GetLineSpan();
            return location.SourceTree.GetText().Lines[fallBackLineSpan.StartLinePosition.Line].ToString();
        }
    }
    public static SourceFileInfo GetSourceLineInfo(this ISymbol symbol, OmniSharpWorkspace workspace)
    {
        var location = symbol.Locations.First();
        var result = location.GetSourceLineInfo(workspace);
        if (symbol is IMethodSymbol)
        {
            var methodName = symbol.ToDisplayParts().FirstOrDefault(p => p.Kind == SymbolDisplayPartKind.MethodName);
            result.ContainingTypeFullName = $"{GetFullTypeName(symbol.ContainingType)}.{methodName}{GetMethodSignature((IMethodSymbol)symbol)}";
        }
        else
        {
            result.ContainingTypeFullName = GetFullTypeName(symbol);
        }
        result.NamespaceName = symbol.ContainingNamespace.GetMetadataName();
        return result;
    }

    private static string GetFullTypeName(ISymbol symbol)
    {
        var result = symbol.Name;
        var ittr = symbol;
        while (ittr.ContainingType != null)
        {
            ittr = ittr.ContainingType;
            result = $"{ittr.Name}+{result}";
        }

        return result;
    }

    private static string GetMethodSignature(IMethodSymbol method)
    {
        var methodParameterTypeNames = new List<string>();
        foreach (var methodParameter in method.Parameters)
        {
            methodParameterTypeNames.Add(methodParameter.Type.Name);
        }

        var result = $"({string.Join(", ", methodParameterTypeNames)})";
        return result;
    }
}