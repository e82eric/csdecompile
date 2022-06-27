using TryOmnisharpExtension.IlSpy;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace TryOmnisharpExtension.FindUsages;

public class FindUsagesCommandFactory : ExternalAssembliesFindUsagesCommandFactory, ICommandFactory<INavigationCommand<FindUsagesResponse>>
{
    private readonly IOmniSharpWorkspace _omniSharpWorkspace;

    public FindUsagesCommandFactory(
        IlSpyTypeUsagesFinder usagesFinder,
        IlSpyMethodUsagesFinder methodUsagesFinder,
        IlSpyPropertyUsagesFinder propertyUsagesFinder,
        IlSpyFieldUsagesFinder fieldUsagesFinder,
        IlSpyVariableUsagesFinder variableUsagesFinder,
        IlSpyEventUsagesFinder eventUsagesFinder,
        IOmniSharpWorkspace omniSharpWorkspace):base(
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