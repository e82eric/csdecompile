using System.Composition;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;
using ISymbol = ICSharpCode.Decompiler.TypeSystem.ISymbol;

namespace TryOmnisharpExtension;

public class IlSpyFindImplementationsCommandFactory : IDecompilerCommandFactory<INavigationCommand<FindImplementationsResponse>>
{
    private readonly IlSpyBaseTypeUsageFinder2 _typeFinder;
    private readonly IlSpyMethodImplementationFinder _methodImplementationFinder;
    private readonly IlSpyPropertyImplementationFinder _propertyImplementationFinder;
    private readonly IlSpyEventImplementationFinder _eventImplementationFinder;

    [ImportingConstructor]
    public IlSpyFindImplementationsCommandFactory(
        IlSpyBaseTypeUsageFinder2 typeFinder,
        IlSpyMethodImplementationFinder methodImplementationFinder,
        IlSpyPropertyImplementationFinder propertyImplementationFinder,
        IlSpyEventImplementationFinder eventImplementationFinder)
    {
        _typeFinder = typeFinder;
        _methodImplementationFinder = methodImplementationFinder;
        _propertyImplementationFinder = propertyImplementationFinder;
        _eventImplementationFinder = eventImplementationFinder;
    }

    public INavigationCommand<FindImplementationsResponse> GetForType(ITypeDefinition typeDefinition, string projectAssemblyFilePath)
    {
        return new FindTypeImplementationsCommand(
            projectAssemblyFilePath,
            typeDefinition,
            _typeFinder);
    }

    public INavigationCommand<FindImplementationsResponse> GetForMethod(IMethod method, string projectAssemblyFilePath)
    {
        return new FindMethodImplementationsCommand(
            projectAssemblyFilePath,
            method,
            _methodImplementationFinder);
    }

    public INavigationCommand<FindImplementationsResponse> GetForProperty(IProperty property, string projectAssemblyFilePath)
    {
        return new FindPropertyImplementationsCommand(
            projectAssemblyFilePath,
            property,
            _propertyImplementationFinder);
    }
        
    public INavigationCommand<FindImplementationsResponse> GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
    {
        return new FindEventImplementationsCommand(
            projectAssemblyFilePath,
            eventSymbol,
            _eventImplementationFinder);
    }
}