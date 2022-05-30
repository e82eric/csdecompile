using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using IlSpy.Analyzer.Extraction;
using TryOmnisharpExtension.FindUsages;

namespace TryOmnisharpExtension;

public class ExternalAssembliesFindUsagesCommandFactory : IDecompilerCommandFactory<INavigationCommand<FindUsagesResponse>>
{
    private readonly IlSpyUsagesFinder _usagesFinder;
    private readonly IlSpyMethodUsagesFinder _methodUsagesFinder;
    private readonly IlSpyPropertyUsagesFinder _propertyUsagesFinder;
    private readonly IlSpyEventUsagesFinder _eventUsagesFinder;

    [ImportingConstructor]
    public ExternalAssembliesFindUsagesCommandFactory(
        IlSpyUsagesFinder usagesFinder,
        IlSpyMethodUsagesFinder methodUsagesFinder,
        IlSpyPropertyUsagesFinder propertyUsagesFinder,
        IlSpyEventUsagesFinder eventUsagesFinder)
    {
        _usagesFinder = usagesFinder;
        _methodUsagesFinder = methodUsagesFinder;
        _propertyUsagesFinder = propertyUsagesFinder;
        _eventUsagesFinder = eventUsagesFinder;
    }

    public INavigationCommand<FindUsagesResponse> GetForType(ITypeDefinition typeDefinition, string projectAssemblyFilePath)
    {
        var result = new FindUsagesCommand(
            projectAssemblyFilePath,
            typeDefinition,
            _usagesFinder);
        return result;
    }

    public INavigationCommand<FindUsagesResponse> GetForMethod(IMethod method, string projectAssemblyFilePath)
    {
        var result = new FindMethodUsagesCommand(projectAssemblyFilePath, method, _methodUsagesFinder);
        return result;
    }

    public INavigationCommand<FindUsagesResponse> GetForProperty(IProperty property, string projectAssemblyFilePath)
    {
        var result = new FindPropertyUsagesCommand(projectAssemblyFilePath, property, _propertyUsagesFinder);
        return result;
    }

    public INavigationCommand<FindUsagesResponse> GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
    {
        var result = new FindEventUsagesCommand(projectAssemblyFilePath, eventSymbol, _eventUsagesFinder);
        return result;
    }
}