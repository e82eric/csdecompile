using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.FindImplementations;

namespace TryOmnisharpExtension.FindUsages;

public class ExternalAssembliesFindUsagesCommandFactory : IDecompilerCommandFactory<INavigationCommand<FindImplementationsResponse>>
{
    private readonly IlSpyTypeUsagesFinder _usagesFinder;
    private readonly IlSpyMethodUsagesFinder _methodUsagesFinder;
    private readonly IlSpyPropertyUsagesFinder _propertyUsagesFinder;
    private readonly IlSpyFieldUsagesFinder _fieldUsagesFinder;
    private readonly IlSpyEventUsagesFinder _eventUsagesFinder;
    private readonly IlSpyVariableUsagesFinder _variableUsagesFinder;

    public ExternalAssembliesFindUsagesCommandFactory(
        IlSpyTypeUsagesFinder usagesFinder,
        IlSpyMethodUsagesFinder methodUsagesFinder,
        IlSpyPropertyUsagesFinder propertyUsagesFinder,
        IlSpyFieldUsagesFinder fieldUsagesFinder,
        IlSpyEventUsagesFinder eventUsagesFinder,
        IlSpyVariableUsagesFinder variableUsagesFinder)
    {
        _usagesFinder = usagesFinder;
        _methodUsagesFinder = methodUsagesFinder;
        _propertyUsagesFinder = propertyUsagesFinder;
        _fieldUsagesFinder = fieldUsagesFinder;
        _eventUsagesFinder = eventUsagesFinder;
        _variableUsagesFinder = variableUsagesFinder;
    }

    public INavigationCommand<FindImplementationsResponse> GetForType(ITypeDefinition typeDefinition, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<ITypeDefinition, FindImplementationsResponse>(
            typeDefinition,
            _usagesFinder);
        return result;
    }

    public INavigationCommand<FindImplementationsResponse> GetForMethod(IMethod method, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, FindImplementationsResponse>(
            method,
            _methodUsagesFinder);
        return result;
    }

    public INavigationCommand<FindImplementationsResponse> GetForProperty(IProperty property, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, FindImplementationsResponse>(
            property,
            _propertyUsagesFinder);
        return result;
    }
    
    public INavigationCommand<FindImplementationsResponse> GetForField(IField field, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, FindImplementationsResponse>(
            field,
            _fieldUsagesFinder);
        return result;
    }

    public INavigationCommand<FindImplementationsResponse> GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, FindImplementationsResponse>(
            eventSymbol,
            _eventUsagesFinder);
        return result;
    }
    
    public INavigationCommand<FindImplementationsResponse> GetForVariable(
        ITypeDefinition containingTypeDefinition,
        AstNode variableNode,
        string sourceText)
    {
        var result = new FindVariableUsagesCommand(containingTypeDefinition, variableNode, _variableUsagesFinder, sourceText);
        return result;
    }
}