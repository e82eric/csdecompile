using CsDecompileLib.FindUsages;
using CsDecompileLib.IlSpy;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.FindImplementations;

public class RoslynFindImplementationsCommandFactory : IlSpyFindImplementationsCommandFactory,
    ICommandFactory<INavigationCommand<LocationsResponse>>
{
    private readonly ICsDecompileWorkspace _csDecompileWorkspace;

    public RoslynFindImplementationsCommandFactory(
        IlSpyUsagesFinderBase<ITypeDefinition> typeFinder,
        IlSpyUsagesFinderBase<IMember> memberImplementationFinder,
        ICsDecompileWorkspace csDecompileWorkspace):base(typeFinder, memberImplementationFinder)
    {
        _csDecompileWorkspace = csDecompileWorkspace;
    }

    public INavigationCommand<LocationsResponse> GetForNamespace(string namespaceString)
    {
        throw new System.NotImplementedException();
    }

    public INavigationCommand<LocationsResponse> GetForEnumField(IField field, string projectAssemblyFilePath)
    {
        throw new System.NotImplementedException();
    }

    public INavigationCommand<LocationsResponse> GetForField(IField field, string projectAssemblyFilePath)
    {
        throw new System.NotImplementedException();
    }

    public INavigationCommand<LocationsResponse> GetForInSource(Microsoft.CodeAnalysis.ISymbol roslynSymbol)
    {
        var result = new RoslynFindImplementationsCommand(roslynSymbol, _csDecompileWorkspace);
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

    public INavigationCommand<LocationsResponse> GetForVariable(ILVariable variable, ITypeDefinition typeDefinition, AstNode syntaxTree,
        string sourceText, string assemblyFilePath)
    {
        throw new System.NotImplementedException();
    }
}