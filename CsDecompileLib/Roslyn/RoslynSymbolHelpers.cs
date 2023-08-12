using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsDecompileLib.IlSpy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CsDecompileLib.Roslyn;

internal static class RoslynSymbolHelpers
{
    private readonly static CachedStringBuilder s_cachedBuilder;
    public static bool IsInterfaceType(this ISymbol symbol) => (symbol as ITypeSymbol)?.IsInterfaceType() == true;
    public static bool IsInterfaceType(this ITypeSymbol symbol) => symbol?.TypeKind == TypeKind.Interface;
    public static bool IsOverridable(this ISymbol symbol) => symbol?.ContainingType?.TypeKind == TypeKind.Class && !symbol.IsSealed && (symbol.IsVirtual || symbol.IsAbstract || symbol.IsOverride);
    
    public static string GetMetadataName(this ISymbol symbol)
    {
        if (symbol == null)
        {
            throw new ArgumentNullException(nameof(symbol));
        }

        if (symbol is INamedTypeSymbol namedTypeSymbol)
        {
            return GetTypeDisplayString(namedTypeSymbol);
        }

        var symbols = new Stack<ISymbol>();

        while (symbol != null)
        {
            if (symbol.Kind == SymbolKind.Assembly ||
                symbol.Kind == SymbolKind.NetModule)
            {
                break;
            }

            if ((symbol as INamespaceSymbol)?.IsGlobalNamespace == true)
            {
                break;
            }

            symbols.Push(symbol);
            symbol = symbol.ContainingSymbol;
        }

        var builder = s_cachedBuilder.Acquire();
        try
        {
            ISymbol current = null, previous = null;

            while (symbols.Count > 0)
            {
                current = symbols.Pop();

                if (previous != null)
                {
                    if (previous.Kind == SymbolKind.NamedType &&
                        current.Kind == SymbolKind.NamedType)
                    {
                        builder.Append('+');
                    }
                    else
                    {
                        builder.Append('.');
                    }
                }

                builder.Append(current.MetadataName);

                previous = current;
            }

            return builder.ToString();
        }
        finally
        {
            s_cachedBuilder.Release(builder);
        }
    }
    public static string GetSymbolName(this ISymbol symbol)
    {
        var topLevelSymbol = symbol.GetTopLevelContainingNamedType();
        return GetTypeDisplayString(topLevelSymbol);
    }

    private static INamedTypeSymbol GetTopLevelContainingNamedType(this ISymbol symbol)
    {
        // Traverse up until we find a named type that is parented by the namespace
        var topLevelNamedType = symbol;
        while (!SymbolEqualityComparer.Default.Equals(topLevelNamedType.ContainingSymbol, symbol.ContainingNamespace) ||
            topLevelNamedType.Kind != SymbolKind.NamedType)
        {
            topLevelNamedType = topLevelNamedType.ContainingSymbol;
        }

        return (INamedTypeSymbol)topLevelNamedType;
    }
    private static string GetTypeDisplayString(INamedTypeSymbol symbol)
    {
        if (symbol.SpecialType != SpecialType.None)
        {
            var specialType = symbol.SpecialType;
            var name = Enum.GetName(typeof(SpecialType), symbol.SpecialType).Replace("_", ".");
            return name;
        }

        if (symbol.IsGenericType)
        {
            symbol = symbol.ConstructUnboundGenericType();
        }

        if (symbol.IsUnboundGenericType)
        {
            // TODO: Is this the best to get the fully metadata name?
            var parts = symbol.ToDisplayParts();
            var filteredParts = parts.Where(x => x.Kind != SymbolDisplayPartKind.Punctuation).ToArray();
            var typeName = new StringBuilder();
            foreach (var part in filteredParts.Take(filteredParts.Length - 1))
            {
                typeName.Append(part.Symbol.Name);
                typeName.Append(".");
            }
            typeName.Append(symbol.MetadataName);

            return typeName.ToString();
        }

        return symbol.ToDisplayString();
    }
    public static bool IsImplementableMember(this ISymbol symbol)
    {
        if (symbol?.ContainingType?.TypeKind == TypeKind.Interface)
        {
            if (symbol.Kind == SymbolKind.Event)
            {
                return true;
            }

            if (symbol.Kind == SymbolKind.Property)
            {
                return true;
            }

            if (symbol.Kind == SymbolKind.Method)
            {
                var methodSymbol = (IMethodSymbol)symbol;
                if (methodSymbol.MethodKind == MethodKind.Ordinary ||
                    methodSymbol.MethodKind == MethodKind.PropertyGet ||
                    methodSymbol.MethodKind == MethodKind.PropertySet)
                {
                    return true;
                }
            }
        }

        return false;
    }
    public static SourceFileInfo GetSourceLineInfo(this Location location, ICsDecompileWorkspace workspace)
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
    public static SourceFileInfo GetSourceLineInfo(this ISymbol symbol, ICsDecompileWorkspace workspace)
    {
        var location = symbol.Locations.First();
        if (location.IsInSource == false)
        {
            return null;
        }
        var result = location.GetSourceLineInfo(workspace);
        if (symbol is INamedTypeSymbol)
        {
            result.ContainingTypeFullName = symbol.GetMetadataName();
            result.ContainingTypeShortName = GetShortName(symbol);
        }
        else if (symbol is IMethodSymbol)
        {
            var methodName = symbol.ToDisplayParts().FirstOrDefault(p => p.Kind == SymbolDisplayPartKind.MethodName);
            result.ContainingTypeFullName = symbol.ContainingType?.GetMetadataName();
        }
        else
        {
            result.ContainingTypeFullName = symbol.ContainingType?.GetMetadataName();
            result.ContainingTypeShortName = GetShortName(symbol);
        }
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
    
    public static string GetShortName(ISymbol enclosingSymbol)
    {
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