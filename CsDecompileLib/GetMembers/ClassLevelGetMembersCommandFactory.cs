using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.TypeSystem;

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
        var containingTypeDefinition = _symbolFinder.FindTypeDefinition(
            request.AssemblyFilePath,
            request.ContainingTypeFullName);
        
        var decompiler = _decompilerFactory.Get(containingTypeDefinition.ParentModule.PEFile.FileName);
        var (syntaxTree, _) = decompiler.Run(request.ContainingTypeFullName);
        var node = _symbolFinder.GetNodeAt(syntaxTree, request.Line, request.Column);
        ITypeDefinition typeDefinition;
        if (node == null)
        {
            typeDefinition = containingTypeDefinition;
        }
        else
        {
            typeDefinition = _symbolFinder.FindParentType(node);
        }

        if (typeDefinition == null)
        {
            typeDefinition = containingTypeDefinition;
        }

        var command = new IlSpyTypeMembersCommand(typeDefinition, _typeMembersFinder);
        return command;
    }
}