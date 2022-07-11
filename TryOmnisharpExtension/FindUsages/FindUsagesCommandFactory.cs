using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.GotoDefinition;
using TryOmnisharpExtension.IlSpy;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace TryOmnisharpExtension.FindUsages;

public class FindUsagesCommandFactory : ExternalAssembliesFindUsagesCommandFactory, ICommandFactory<INavigationCommand<FindImplementationsResponse>>
{
    private readonly IOmniSharpWorkspace _omniSharpWorkspace;

    public FindUsagesCommandFactory(
        IlSpyUsagesFinderBase<ITypeDefinition> usagesFinder,
        IlSpyUsagesFinderBase<IMember> methodUsagesFinder,
        IlSpyUsagesFinderBase<IMember> propertyUsagesFinder,
        IlSpyUsagesFinderBase<IMember> fieldUsagesFinder,
        IlSpyUsagesFinderBase<IMember> enumFieldUsagesFinder,
        IlSpyVariableUsagesFinder variableUsagesFinder,
        IlSpyUsagesFinderBase<IMember> eventUsagesFinder,
        IOmniSharpWorkspace omniSharpWorkspace):base(
            usagesFinder,
            methodUsagesFinder,
            propertyUsagesFinder,
            fieldUsagesFinder,
            enumFieldUsagesFinder,
            eventUsagesFinder,
            variableUsagesFinder)
    {
        _omniSharpWorkspace = omniSharpWorkspace;
    }

    public INavigationCommand<FindImplementationsResponse> GetForInSource(ISymbol roslynSymbol)
    {
        var result = new RoslynFindUsagesCommand(roslynSymbol, _omniSharpWorkspace);
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

    public INavigationCommand<FindImplementationsResponse> GetForVariable(ILVariable variable, ITypeDefinition typeDefinition, SyntaxTree syntaxTree,
        string sourceText, string assemblyFilePath)
    {
        throw new System.NotImplementedException();
    }
}