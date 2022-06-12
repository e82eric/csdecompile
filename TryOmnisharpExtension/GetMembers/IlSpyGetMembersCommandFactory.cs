using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.GetMembers;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension;

[Export]
public class IlSpyGetMembersCommandFactory
{
    private readonly IlSpySymbolFinder _symbolFinder;
    private readonly IlSpyTypeMembersFinder _typeMembersFinder;

    [ImportingConstructor]
    public IlSpyGetMembersCommandFactory(
        IlSpySymbolFinder symbolFinder,
        IlSpyTypeMembersFinder typeMembersFinder)
    {
        _symbolFinder = symbolFinder;
        _typeMembersFinder = typeMembersFinder;
    }
        
    public INavigationCommand<GetTypeMembersResponse>Find(DecompiledLocationRequest request)
    {
        var symbolAtLocation = _symbolFinder.FindTypeDefinition(
            request.AssemblyFilePath,
            request.ContainingTypeFullName);

        var typeDefinition = SymbolHelper.FindContainingType(symbolAtLocation);

        var command = new IlSpyTypeMembersCommand(typeDefinition, _typeMembersFinder);
        return command;
    }
}