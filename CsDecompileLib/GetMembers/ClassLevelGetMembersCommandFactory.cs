using CsDecompileLib.IlSpy;

namespace CsDecompileLib.GetMembers;

public class ClassLevelGetMembersCommandFactory : INavigationCommandFactory<INavigationCommand<FindImplementationsResponse>>
{
    private readonly IlSpySymbolFinder _symbolFinder;
    private readonly IlSpyTypeMembersFinder _typeMembersFinder;
    private readonly DecompilerFactory _decompilerFactory;

    public ClassLevelGetMembersCommandFactory(
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
        var (syntaxTree, _) = decompiler.Run(request.ContainingTypeFullName);
        var node = _symbolFinder.GetNodeAt(syntaxTree, request.Line, request.Column);
        var typeDefinition = _symbolFinder.FindParentType(node);

        var command = new IlSpyTypeMembersCommand(typeDefinition, _typeMembersFinder);
        return command;
    }
}