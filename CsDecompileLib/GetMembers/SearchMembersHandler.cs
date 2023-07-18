using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace CsDecompileLib.GetMembers;

public class SearchMembersHandler : HandlerBase<MemberSearchRequest, FindImplementationsResponse>
{
    private readonly MemberSearcher _memberSearcher;
    private readonly IlSpySymbolFinder _symbolFinder;
    private readonly DecompilerFactory _decompilerFactory;
    private readonly ICsDecompileWorkspace _csDecompileWorkspace;

    public SearchMembersHandler(
        MemberSearcher memberSearcher,
        IlSpySymbolFinder symbolFinder,
        DecompilerFactory decompilerFactory,
        ICsDecompileWorkspace csDecompileWorkspace)
    {
        _symbolFinder = symbolFinder;
        _decompilerFactory = decompilerFactory;
        _memberSearcher = memberSearcher;
        _csDecompileWorkspace = csDecompileWorkspace;
    }

    public override async Task<ResponsePacket<FindImplementationsResponse>> Handle(MemberSearchRequest request)
    {
        var body = new FindImplementationsResponse();
        var roslynSymbols = new List<ISymbol>();
        foreach (var project in _csDecompileWorkspace.CurrentSolution.Projects)
        {
            var compilation = await project.GetCompilationAsync();
            if (compilation != null)
            {
                var projectRoslynSymbols = compilation
                    .GetSymbolsWithName(s => s.Contains(request.MemberSearchString), SymbolFilter.Member)
                    .Where(s => s.ContainingAssembly.Name.Contains(request.AssemblySearchString));
                roslynSymbols.AddRange(projectRoslynSymbols);
            }
        }

        foreach (var roslynSymbol in roslynSymbols)
        {
            var sourceFileInfo = roslynSymbol.GetSourceLineInfo(_csDecompileWorkspace);
            sourceFileInfo.ContainingTypeShortName = GetShortName(roslynSymbol);
            body.Implementations.Add(sourceFileInfo);
        }

        var memberInfos = _memberSearcher.SearchForMembers(
            request.AssemblySearchString,
            request.MemberSearchString);

        foreach (var memberInfo in memberInfos)
        {
            var containingTypeDefinition = _symbolFinder.FindTypeDefinition(
                memberInfo.assemblyFilePath,
                memberInfo.containingTypeFullName);

            var command = new IlSpyTypeMembersCommand(containingTypeDefinition,
                new IlSpyTypeMembersFinder(new TypeMembersByNameFinder(memberInfo.memberName), _decompilerFactory));

            var currentResult = await command.Execute();

            foreach (var bodyImplementation in currentResult.Body.Implementations)
            {
                body.Implementations.Add(bodyImplementation);
            }
        }

        var result = ResponsePacket.Ok(body);

        return result;
    }
    private string GetShortName(ISymbol enclosingSymbol)
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
}