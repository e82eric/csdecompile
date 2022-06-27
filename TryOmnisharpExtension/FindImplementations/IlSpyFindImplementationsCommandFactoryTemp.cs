using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindImplementations;

public class IlSpyFindImplementationsCommandFactoryTemp : IlSpyFindImplementationsCommandFactory,
    ICommandFactory<INavigationCommand<FindImplementationsResponse>>
{
    private readonly IOmniSharpWorkspace _omniSharpWorkspace;

    public IlSpyFindImplementationsCommandFactoryTemp(
        IlSpyBaseTypeUsageFinder typeFinder,
        IlSpyMemberImplementationFinder memberImplementationFinder,
        IOmniSharpWorkspace omniSharpWorkspace):base(typeFinder, memberImplementationFinder)
    {
        _omniSharpWorkspace = omniSharpWorkspace;
    }

    public INavigationCommand<FindImplementationsResponse> GetForField(IField field, string projectAssemblyFilePath)
    {
        throw new System.NotImplementedException();
    }

    public INavigationCommand<FindImplementationsResponse> GetForInSource(Microsoft.CodeAnalysis.ISymbol roslynSymbol)
    {
        var result = new RosylynFindImplementationsCommand(roslynSymbol, _omniSharpWorkspace);
        return result;
    }
}