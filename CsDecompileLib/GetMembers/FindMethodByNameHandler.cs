using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace CsDecompileLib.GetMembers;

public class FindMethodByNameHandler : HandlerBase<FindMethodByNameRequest, FindImplementationsResponse>
{
    private readonly AllTypesRepositoryByName _typesRepository;
    private readonly IlSpySymbolFinder _symbolFinder;
    private readonly DecompilerFactory _decompilerFactory;
    private readonly ICsDecompileWorkspace _csDecompileWorkspace;

    public FindMethodByNameHandler(
        AllTypesRepositoryByName typesRepository,
        IlSpySymbolFinder symbolFinder,
        DecompilerFactory decompilerFactory,
        ICsDecompileWorkspace csDecompileWorkspace)
    {
        _symbolFinder = symbolFinder;
        _decompilerFactory = decompilerFactory;
        _csDecompileWorkspace = csDecompileWorkspace;
        _typesRepository = typesRepository;
    }

    public override async Task<ResponsePacket<FindImplementationsResponse>> Handle(FindMethodByNameRequest request)
    {
        var body = new FindImplementationsResponse();
        var roslynSymbols = new List<ISymbol>();
        foreach (var project in _csDecompileWorkspace.CurrentSolution.Projects)
        {
            var compilation = await project.GetCompilationAsync();
            if (compilation != null)
            {
                var projectRoslynSymbols = compilation
                    .GetSymbolsWithName(s => request.MethodName == s, SymbolFilter.Member)
                    .Where(s =>
                        s.ContainingType.MetadataName == request.TypeName &&
                        s.ContainingNamespace.Name == request.NamespaceName);
                roslynSymbols.AddRange(projectRoslynSymbols);
            }
        }

        foreach (var roslynSymbol in roslynSymbols)
        {
            var sourceFileInfo = roslynSymbol.GetSourceLineInfo(_csDecompileWorkspace);
            sourceFileInfo.ContainingTypeShortName = GetShortName(roslynSymbol);
            body.Implementations.Add(sourceFileInfo);
        }
        
        var namespaces = _typesRepository.GetAllTypes(
            request.NamespaceName,
            request.TypeName);

        foreach (var namespaceDefinition in namespaces)
        {
            var containingTypeDefinition = _symbolFinder.FindTypeDefinition(
                namespaceDefinition.AssemblyFilePath,
                namespaceDefinition.ContainingTypeFullName);

            ITypeDefinition typeDefinition = containingTypeDefinition;

            var command = new IlSpyTypeMembersCommand(typeDefinition,
                new IlSpyTypeMembersFinder(new TypeMembersByNameFinder(request.MethodName), _decompilerFactory));

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