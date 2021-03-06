using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using OmniSharp;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindImplementations;

[Export(typeof(ICommandFactory<INavigationCommand<FindImplementationsResponse>>))]
public class IlSpyFindImplementationsCommandFactoryTemp : IlSpyFindImplementationsCommandFactory,
    ICommandFactory<INavigationCommand<FindImplementationsResponse>>
{
    private readonly OmniSharpWorkspace _omniSharpWorkspace;

    [ImportingConstructor]
    public IlSpyFindImplementationsCommandFactoryTemp(
        IlSpyBaseTypeUsageFinder typeFinder,
        IlSpyMemberImplementationFinder memberImplementationFinder,
        OmniSharpWorkspace omniSharpWorkspace):base(typeFinder, memberImplementationFinder)
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