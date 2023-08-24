using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using CsDecompileLib.Roslyn;
using Microsoft.CodeAnalysis;

namespace CsDecompileLib.GetMembers;

public class RoslynAllTypesRepository
{
    private readonly ICsDecompileWorkspace _csDecompileWorkspace;

    public RoslynAllTypesRepository(ICsDecompileWorkspace csDecompileWorkspace)
    {
        _csDecompileWorkspace = csDecompileWorkspace;
    }

    public async Task<IEnumerable<ResponseLocation>> GetAllTypes(string searchString)
    {
        var result = new List<ResponseLocation>();

        var roslynSymbols = new List<ISymbol>();
        foreach (var project in _csDecompileWorkspace.CurrentSolution.Projects)
        {
            var compilation = await project.GetCompilationAsync();
            if (compilation != null)
            {
                var namespaces = FindNamespaces(compilation.GlobalNamespace, searchString);

                foreach (var namespaceSymbol in namespaces)
                {
                    var members = namespaceSymbol.GetMembers();
                    foreach (var namespaceMember in members)
                    {
                        if (namespaceMember is INamedTypeSymbol)
                        {
                            if (namespaceMember.Locations.First().IsInSource)
                            {
                                roslynSymbols.Add(namespaceMember);
                            }
                        }
                    }
                }

            }
        }

        foreach (var roslynSymbol in roslynSymbols)
        {
            var sourceFileInfo = roslynSymbol.GetSourceLineInfo(_csDecompileWorkspace);
            sourceFileInfo.ContainingTypeShortName = RoslynSymbolHelpers.GetShortName(roslynSymbol);
            result.Add(sourceFileInfo);
        }

        return result;
    }

    static IEnumerable<INamespaceSymbol> FindNamespaces(INamespaceSymbol root, string targetName)
    {
        var namespaces = new List<INamespaceSymbol>();

        foreach (var member in root.GetMembers())
        {
            if (member is INamespaceSymbol ns)
            {
                if (ns.ToDisplayString() == targetName)
                {
                    namespaces.Add(ns);
                }

                namespaces.AddRange(FindNamespaces(ns, targetName)); // Recursively search nested namespaces
            }
        }

        return namespaces;
    }
}