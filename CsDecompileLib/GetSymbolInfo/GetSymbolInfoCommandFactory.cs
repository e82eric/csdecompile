using CsDecompileLib.GotoDefinition;
using CsDecompileLib.IlSpy;
using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GetSymbolInfo;

public class GetSymbolInfoCommandFactory : ICommandFactory<INavigationCommand<SymbolInfo>>
{
    private readonly IDecompileWorkspace _decompileWorkspace;

    public GetSymbolInfoCommandFactory(IDecompileWorkspace decompileWorkspace)
    {
        _decompileWorkspace = decompileWorkspace;
    }
    public INavigationCommand<SymbolInfo> GetForFileNotFound(string filePath)
    {
        var result = new FileNotFoundCommand<SymbolInfo>(filePath);
        return result;
    }
        
    public INavigationCommand<SymbolInfo> SymbolNotFoundAtLocation(string filePath, int line, int column)
    {
        var result = new SymbolNotFoundAtLocationCommand<SymbolInfo>(filePath, line, column);
        return result;
    }
    public INavigationCommand<SymbolInfo> GetForInSource(Microsoft.CodeAnalysis.ISymbol roslynSymbol)
    {
        return new RoslynSymbolInfoCommand(roslynSymbol);
    }

    public INavigationCommand<SymbolInfo> GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
    {
        var result = new IlSpyMemberSymbolInfoCommand(eventSymbol);
        return result;
    }

    public INavigationCommand<SymbolInfo> GetForType(ITypeDefinition typeDefinition, string assemblyFilePath)
    {
        if (typeDefinition.ParentModule == null)
        {
            var peFile = _decompileWorkspace.GetAssembly(assemblyFilePath);
            return new UnresolvedTypeSymbolInfoCommand(peFile, typeDefinition);
        }
        var result = new IlSpyTypeDefinitionSymbolInfoCommand(typeDefinition);
        return result;
    }

    public INavigationCommand<SymbolInfo> GetForMethod(IMethod method, string assemblyFilePath)
    {
        if (method.ParentModule == null)
        {
            var peFile = _decompileWorkspace.GetAssembly(assemblyFilePath);
            return new UnresolvedMemberSymbolInfoCommand(peFile, method);
        }
        var result = new IlSpyMethodSymbolInfoCommand(method);
        return result;
    }

    public INavigationCommand<SymbolInfo> GetForEnumField(IField field, string projectAssemblyFilePath)
    {
        throw new System.NotImplementedException();
    }

    public INavigationCommand<SymbolInfo> GetForField(IField field, string assemblyFilePath)
    {
        if (field.ParentModule == null)
        {
            var peFile = _decompileWorkspace.GetAssembly(assemblyFilePath);
            return new UnresolvedMemberSymbolInfoCommand(peFile, field);
        }
        var result = new IlSpyMemberSymbolInfoCommand(field);
        return result;
    }

    public INavigationCommand<SymbolInfo> GetForProperty(IProperty property, string assemblyFilePath)
    {
        if (property.ParentModule == null)
        {
            var peFile = _decompileWorkspace.GetAssembly(assemblyFilePath);
            return new UnresolvedMemberSymbolInfoCommand(peFile, property);
        }
        var result = new IlSpyMemberSymbolInfoCommand(property);
        return result;
    }

    public INavigationCommand<SymbolInfo> GetForVariable(
        ILVariable variable,
        ITypeDefinition typeDefinition,
        AstNode syntaxTree,
        string sourceText,
        string assemblyFilePath)
    {
        var result = new IlSpyVariableSymbolInfoCommand(variable);
        return result;
    }
}