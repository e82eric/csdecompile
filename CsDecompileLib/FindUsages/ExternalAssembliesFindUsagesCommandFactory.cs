using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace CsDecompileLib.FindUsages;

public class ExternalAssembliesFindUsagesCommandFactory : IDecompilerCommandFactory<INavigationCommand<LocationsResponse>>
{
    private readonly IlSpyUsagesFinderBase<ITypeDefinition> _usagesFinder;
    private readonly IlSpyUsagesFinderBase<IMember> _methodUsagesFinder;
    private readonly IlSpyUsagesFinderBase<IMember> _propertyUsagesFinder;
    private readonly IlSpyUsagesFinderBase<IMember> _fieldUsagesFinder;
    private readonly IlSpyUsagesFinderBase<IMember> _eventUsagesFinder;
    private readonly IlSpyUsagesFinderBase<IMember> _enumFieldUsagesFinder;
    private readonly IlSpyVariableUsagesFinder _variableUsagesFinder;

    public ExternalAssembliesFindUsagesCommandFactory(
        IlSpyUsagesFinderBase<ITypeDefinition> usagesFinder,
        IlSpyUsagesFinderBase<IMember> methodUsagesFinder,
        IlSpyUsagesFinderBase<IMember> propertyUsagesFinder,
        IlSpyUsagesFinderBase<IMember> fieldUsagesFinder,
        IlSpyUsagesFinderBase<IMember> enumFieldUsagesFinder,
        IlSpyUsagesFinderBase<IMember> eventUsagesFinder,
        IlSpyVariableUsagesFinder variableUsagesFinder)
    {
        _enumFieldUsagesFinder = enumFieldUsagesFinder;
        _usagesFinder = usagesFinder;
        _methodUsagesFinder = methodUsagesFinder;
        _propertyUsagesFinder = propertyUsagesFinder;
        _fieldUsagesFinder = fieldUsagesFinder;
        _eventUsagesFinder = eventUsagesFinder;
        _variableUsagesFinder = variableUsagesFinder;
    }

    public INavigationCommand<LocationsResponse> GetForType(ITypeDefinition typeDefinition, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<ITypeDefinition, LocationsResponse>(
            typeDefinition,
            _usagesFinder);
        return result;
    }

    public INavigationCommand<LocationsResponse> GetForMethod(IMethod method, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, LocationsResponse>(
            method,
            _methodUsagesFinder);
        return result;
    }

    public INavigationCommand<LocationsResponse> GetForProperty(IProperty property, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, LocationsResponse>(
            property,
            _propertyUsagesFinder);
        return result;
    }
    
    public INavigationCommand<LocationsResponse> GetForField(IField field, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, LocationsResponse>(
            field,
            _fieldUsagesFinder);
        return result;
    }
    
    public INavigationCommand<LocationsResponse> GetForEnumField(IField field, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, LocationsResponse>(
            field,
            _enumFieldUsagesFinder);
        return result;
    }

    public INavigationCommand<LocationsResponse> GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, LocationsResponse>(
            eventSymbol,
            _eventUsagesFinder);
        return result;
    }
    
    public INavigationCommand<LocationsResponse> GetForVariable(ILVariable variable, ITypeDefinition typeDefinition, AstNode methodNode,
        string sourceText, string assemblyFilePath)
    {
        var result = new FindVariableUsagesCommand(
            typeDefinition,
            methodNode,
            variable,
            _variableUsagesFinder,
            sourceText);
        return result;
    }
}