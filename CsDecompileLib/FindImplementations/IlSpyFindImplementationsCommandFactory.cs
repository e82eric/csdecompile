using CsDecompileLib.FindUsages;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindImplementations;

public class IlSpyFindImplementationsCommandFactory : IDecompilerCommandFactory<INavigationCommand<FindImplementationsResponse>>
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

    public INavigationCommand<FindImplementationsResponse> GetForType(ITypeDefinition typeDefinition, string projectAssemblyFilePath)
    {
        if (typeDefinition.IsSealed)
        {
            return new NoOpCommand<FindImplementationsResponse>();
        }
        
        var result = new IlSpyUsagesCommand<ITypeDefinition, FindImplementationsResponse>(
            typeDefinition,
            _typeFinder);
        return result;
    }

    public INavigationCommand<FindImplementationsResponse> GetForMethod(IMethod method, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, FindImplementationsResponse>(
            method,
            _memberImplementationFinder);
        return result;
    }

    public INavigationCommand<FindImplementationsResponse> GetForProperty(IProperty property, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, FindImplementationsResponse>(
            property,
            _memberImplementationFinder);
        return result;
    }
        
    public INavigationCommand<FindImplementationsResponse> GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
    {
        var result = new IlSpyUsagesCommand<IMember, FindImplementationsResponse>(
            eventSymbol,
            _memberImplementationFinder);
        return result;
    }
}