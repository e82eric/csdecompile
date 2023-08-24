using CsDecompileLib.FindUsages;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindImplementations;

public class IlSpyFindImplementationsCommandFactory : IDecompilerCommandFactory<INavigationCommand<LocationsResponse>>
{
    private readonly IlSpyUsagesFinderBase<ITypeDefinition> _typeFinder;
    private readonly IlSpyUsagesFinderBase<IMember> _memberImplementationFinder;

    public IlSpyFindImplementationsCommandFactory(
        IlSpyUsagesFinderBase<ITypeDefinition> typeFinder,
        IlSpyUsagesFinderBase<IMember> memberImplementationFinder)
    {
        _typeFinder = typeFinder;
        _memberImplementationFinder = memberImplementationFinder;
    }

    public INavigationCommand<LocationsResponse> GetForType(ITypeDefinition typeDefinition, string projectAssemblyFilePath)
    {
        if (typeDefinition.IsSealed)
        {
            return new NoOpCommand<LocationsResponse>();
        }
        
        var result = new IlSpyUsagesCommand<ITypeDefinition, LocationsResponse>(
            typeDefinition,
            _typeFinder);
        return result;
    }

    public INavigationCommand<LocationsResponse> GetForMethod(IMethod method, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, LocationsResponse>(
            method,
            _memberImplementationFinder);
        return result;
    }

    public INavigationCommand<LocationsResponse> GetForProperty(IProperty property, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, LocationsResponse>(
            property,
            _memberImplementationFinder);
        return result;
    }
        
    public INavigationCommand<LocationsResponse> GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, LocationsResponse>(
            eventSymbol,
            _memberImplementationFinder);
        return result;
    }
}