using System.Composition;
using OmniSharp;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace TryOmnisharpExtension.FindUsages;

[Export(typeof(ICommandFactory<INavigationCommand<FindUsagesResponse>>))]
public class FindUsagesCommandFactory : ExternalAssembliesFindUsagesCommandFactory, ICommandFactory<INavigationCommand<FindUsagesResponse>>
{
    private readonly OmniSharpWorkspace _omniSharpWorkspace;

    [ImportingConstructor]
    public FindUsagesCommandFactory(
        IlSpyTypeUsagesFinder usagesFinder,
        IlSpyMethodUsagesFinder methodUsagesFinder,
        IlSpyPropertyUsagesFinder propertyUsagesFinder,
        IlSpyFieldUsagesFinder fieldUsagesFinder,
        IlSpyVariableUsagesFinder variableUsagesFinder,
        IlSpyEventUsagesFinder eventUsagesFinder,
        OmniSharpWorkspace omniSharpWorkspace):base(
            usagesFinder,
            methodUsagesFinder,
            propertyUsagesFinder,
            fieldUsagesFinder,
            eventUsagesFinder,
            variableUsagesFinder)
    {
        _omniSharpWorkspace = omniSharpWorkspace;
    }

    public INavigationCommand<FindUsagesResponse> GetForInSource(ISymbol roslynSymbol)
    {
        var result = new RoslynFindUsagesCommand(roslynSymbol, _omniSharpWorkspace);
        return result;
    }
}