using System.Collections.Generic;
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
                var projectRoslynSymbols = compilation
                    .GetSymbolsWithName(s => s.Contains(searchString), SymbolFilter.Type);

                foreach (var projectRoslynSymbol in projectRoslynSymbols)
                {
                    if (projectRoslynSymbol.ContainingNamespace.MetadataName == searchString)
                    {
                        roslynSymbols.Add(projectRoslynSymbol);
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
}