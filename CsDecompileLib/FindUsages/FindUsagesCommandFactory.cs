using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.TypeSystem;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace CsDecompileLib.FindUsages;

public class FindUsagesCommandFactory : ExternalAssembliesFindUsagesCommandFactory, ICommandFactory<INavigationCommand<LocationsResponse>>
{
    private readonly ICsDecompileWorkspace _csDecompileWorkspace;

    public FindUsagesCommandFactory(
        IlSpyUsagesFinderBase<ITypeDefinition> usagesFinder,
        IlSpyUsagesFinderBase<IMember> methodUsagesFinder,
        IlSpyUsagesFinderBase<IMember> propertyUsagesFinder,
        IlSpyUsagesFinderBase<IMember> fieldUsagesFinder,
        IlSpyUsagesFinderBase<IMember> enumFieldUsagesFinder,
        IlSpyVariableUsagesFinder variableUsagesFinder,
        IlSpyUsagesFinderBase<IMember> eventUsagesFinder,
        ICsDecompileWorkspace csDecompileWorkspace):base(
            usagesFinder,
            methodUsagesFinder,
            propertyUsagesFinder,
            fieldUsagesFinder,
            enumFieldUsagesFinder,
            eventUsagesFinder,
            variableUsagesFinder)
    {
        _csDecompileWorkspace = csDecompileWorkspace;
    }

    public INavigationCommand<LocationsResponse> GetForNamespace(string namespaceString)
    {
        throw new System.NotImplementedException();
    }

    public INavigationCommand<LocationsResponse> GetForInSource(ISymbol roslynSymbol)
    {
        var result = new RoslynFindUsagesCommand(roslynSymbol, _csDecompileWorkspace);
        return result;
    }

    public INavigationCommand<LocationsResponse> GetForFileNotFound(string filePath)
    {
        var result = new FileNotFoundCommand<LocationsResponse>(filePath);
        return result;
    }

    public INavigationCommand<LocationsResponse> SymbolNotFoundAtLocation(string filePath, int line, int column)
    {
        var result = new SymbolNotFoundAtLocationCommand<LocationsResponse>(filePath, line, column);
        return result;
    }
}