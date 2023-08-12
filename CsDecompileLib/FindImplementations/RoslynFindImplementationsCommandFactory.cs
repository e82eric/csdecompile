using CsDecompileLib.FindUsages;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindImplementations;

public class RoslynFindImplementationsCommandFactory : IlSpyFindImplementationsCommandFactory,
    ICommandFactory<INavigationCommand<FindImplementationsResponse>>
{
    private readonly ICsDecompileWorkspace _csDecompileWorkspace;

    public RoslynFindImplementationsCommandFactory(
        IlSpyUsagesFinderBase<ITypeDefinition> typeFinder,
        IlSpyUsagesFinderBase<IMember> memberImplementationFinder,
        ICsDecompileWorkspace csDecompileWorkspace):base(typeFinder, memberImplementationFinder)
    {
        _csDecompileWorkspace = csDecompileWorkspace;
    }

    public INavigationCommand<FindImplementationsResponse> GetForNamespace(string namespaceString)
    {
        throw new System.NotImplementedException();
    }

    public INavigationCommand<FindImplementationsResponse> GetForEnumField(IField field, string projectAssemblyFilePath)
    {
        throw new System.NotImplementedException();
    }

    public INavigationCommand<FindImplementationsResponse> GetForField(IField field, string projectAssemblyFilePath)
    {
        throw new System.NotImplementedException();
    }

    public INavigationCommand<FindImplementationsResponse> GetForInSource(Microsoft.CodeAnalysis.ISymbol roslynSymbol)
    {
        var result = new RoslynFindImplementationsCommand(roslynSymbol, _csDecompileWorkspace);
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

    public INavigationCommand<FindImplementationsResponse> GetForVariable(ILVariable variable, ITypeDefinition typeDefinition, AstNode syntaxTree,
        string sourceText, string assemblyFilePath)
    {
        throw new System.NotImplementedException();
    }
}