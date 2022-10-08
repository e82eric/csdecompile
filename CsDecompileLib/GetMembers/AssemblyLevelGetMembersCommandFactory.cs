using CsDecompileLib.IlSpy;

namespace CsDecompileLib.GetMembers;

public class AssemblyLevelGetMembersCommandFactory : INavigationCommandFactory<INavigationCommand<FindImplementationsResponse>>
{
    private readonly IlSpySymbolFinder _symbolFinder;
    private readonly IlSpyTypeMembersFinder _typeMembersFinder;
    private readonly DecompilerFactory _decompilerFactory;

    public AssemblyLevelGetMembersCommandFactory(
        IlSpySymbolFinder symbolFinder,
        IlSpyTypeMembersFinder typeMembersFinder,
        DecompilerFactory decompilerFactory)
    {
        _symbolFinder = symbolFinder;
        _typeMembersFinder = typeMembersFinder;
        _decompilerFactory = decompilerFactory;
    }
        
    public INavigationCommand<FindImplementationsResponse>Find(DecompiledLocationRequest request)
    {
        var decompiler = _decompilerFactory.Get(request.AssemblyFilePath);
        var (syntaxTree, _) = decompiler.DecompileWholeModule();
        var node = _symbolFinder.GetNodeAt(syntaxTree, request.Line, request.Column);
        var typeDefinition = _symbolFinder.FindParentType(node);

        var command = new IlSpyTypeMembersCommand(typeDefinition, _typeMembersFinder);
        return command;
    }
}