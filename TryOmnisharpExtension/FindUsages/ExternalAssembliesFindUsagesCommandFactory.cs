using System.Composition;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.FindUsages;

public class ExternalAssembliesFindUsagesCommandFactory : IDecompilerCommandFactory<INavigationCommand<FindUsagesResponse>>
{
    private readonly IlSpyTypeUsagesFinder _usagesFinder;
    private readonly IlSpyMethodUsagesFinder _methodUsagesFinder;
    private readonly IlSpyPropertyUsagesFinder _propertyUsagesFinder;
    private readonly IlSpyFieldUsagesFinder _fieldUsagesFinder;
    private readonly IlSpyEventUsagesFinder _eventUsagesFinder;
    private readonly IlSpyVariableUsagesFinder _variableUsagesFinder;

    [ImportingConstructor]
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

    public INavigationCommand<FindUsagesResponse> GetForType(ITypeDefinition typeDefinition, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<ITypeDefinition, FindUsagesResponse>(
            typeDefinition,
            _usagesFinder);
        return result;
    }

    public INavigationCommand<FindUsagesResponse> GetForMethod(IMethod method, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, FindUsagesResponse>(
            method,
            _methodUsagesFinder);
        return result;
    }

    public INavigationCommand<FindUsagesResponse> GetForProperty(IProperty property, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, FindUsagesResponse>(
            property,
            _propertyUsagesFinder);
        return result;
    }
    
    public INavigationCommand<FindUsagesResponse> GetForField(IField field, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, FindUsagesResponse>(
            field,
            _fieldUsagesFinder);
        return result;
    }

    public INavigationCommand<FindUsagesResponse> GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, FindUsagesResponse>(
            eventSymbol,
            _eventUsagesFinder);
        return result;
    }
    
    public INavigationCommand<FindUsagesResponse> GetForVariable(
        ITypeDefinition containingTypeDefinition,
        AstNode variableNode,
        string sourceText)
    {
        var result = new FindVariableUsagesCommand(containingTypeDefinition, variableNode, _variableUsagesFinder, sourceText);
        return result;
    }
}