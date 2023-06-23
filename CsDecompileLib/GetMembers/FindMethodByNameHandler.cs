using System.Threading.Tasks;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GetMembers;

public class FindMethodByNameHandler : HandlerBase<FindMethodByNameRequest, FindImplementationsResponse>
{
    private readonly AllTypesRepositoryByName _typesRepository;
    private readonly IlSpySymbolFinder _symbolFinder;
    private readonly DecompilerFactory _decompilerFactory;

    public FindMethodByNameHandler(
        AllTypesRepositoryByName typesRepository,
        IlSpySymbolFinder symbolFinder,
        DecompilerFactory decompilerFactory)
    {
        _symbolFinder = symbolFinder;
        _decompilerFactory = decompilerFactory;
        _typesRepository = typesRepository;
    }

    public override async Task<ResponsePacket<FindImplementationsResponse>> Handle(FindMethodByNameRequest request)
    {
        var namespaces = _typesRepository.GetAllTypes(
            request.NamespaceName,
            request.TypeName);

        var body = new FindImplementationsResponse();
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
}