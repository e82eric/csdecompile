using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using IlSpy.Analyzer.Extraction;
using OmniSharp;
using TryOmnisharpExtension.FindUsages;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace TryOmnisharpExtension;

[Export(typeof(ICommandFactory<INavigationCommand<FindUsagesResponse>>))]
public class FindUsagesCommandFactory : ICommandFactory<INavigationCommand<FindUsagesResponse>>
{
    private readonly IlSpyUsagesFinder _usagesFinder;
    private readonly IlSpyMethodUsagesFinder _methodUsagesFinder;
    private readonly IlSpyPropertyUsagesFinder _propertyUsagesFinder;
    private readonly OmniSharpWorkspace _omniSharpWorkspace;
    private readonly IlSpyFieldUsagesFinder _fieldUsagesFinder;

    [ImportingConstructor]
    public FindUsagesCommandFactory(
        IlSpyUsagesFinder usagesFinder,
        IlSpyMethodUsagesFinder methodUsagesFinder,
        IlSpyPropertyUsagesFinder propertyUsagesFinder,
        IlSpyFieldUsagesFinder fieldUsagesFinder,
        OmniSharpWorkspace omniSharpWorkspace)
    {
        _fieldUsagesFinder = fieldUsagesFinder;
        _usagesFinder = usagesFinder;
        _methodUsagesFinder = methodUsagesFinder;
        _propertyUsagesFinder = propertyUsagesFinder;
        _omniSharpWorkspace = omniSharpWorkspace;
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

    public INavigationCommand<FindUsagesResponse> GetForField(IField field, string projectAssemblyFilePath)
    {
        var result = new FindFieldUsagesCommand(projectAssemblyFilePath, field, _fieldUsagesFinder);
        return result;
    }

    public INavigationCommand<FindUsagesResponse> GetForProperty(IProperty property, string projectAssemblyFilePath)
    {
        var result = new FindPropertyUsagesCommand(projectAssemblyFilePath, property, _propertyUsagesFinder);
        return result;
    }

    public INavigationCommand<FindUsagesResponse> GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
    {
        throw new System.NotImplementedException();
    }

    public INavigationCommand<FindUsagesResponse> GetForInSource(ISymbol roslynSymbol)
    {
        var result = new RoslynFindUsagesCommand(roslynSymbol, _omniSharpWorkspace);
        return result;
    }
}