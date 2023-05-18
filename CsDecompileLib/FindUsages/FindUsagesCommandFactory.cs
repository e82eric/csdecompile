using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.TypeSystem;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace CsDecompileLib.FindUsages;

public class FindUsagesCommandFactory : ExternalAssembliesFindUsagesCommandFactory, ICommandFactory<INavigationCommand<FindImplementationsResponse>>
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

    public INavigationCommand<FindImplementationsResponse> GetForInSource(ISymbol roslynSymbol)
    {
        var result = new RoslynFindUsagesCommand(roslynSymbol, _csDecompileWorkspace);
        return result;
    }

    public INavigationCommand<FindImplementationsResponse> GetForFileNotFound(string filePath)
    {
        var result = new FileNotFoundCommand<FindImplementationsResponse>(filePath);
        return result;
    }

    public INavigationCommand<FindImplementationsResponse> SymbolNotFoundAtLocation(string filePath, int line, int column)
    {
        var result = new SymbolNotFoundAtLocationCommand<FindImplementationsResponse>(filePath, line, column);
        return result;
    }
}