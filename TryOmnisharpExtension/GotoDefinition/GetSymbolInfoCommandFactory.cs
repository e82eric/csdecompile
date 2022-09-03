using System;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.Roslyn;

namespace TryOmnisharpExtension.GotoDefinition;

public class GetSymbolInfoCommandFactory : ICommandFactory<INavigationCommand<SymbolInfo>>
{
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
        var result = new IlSpyTypeDefinitionSymbolInfoCommand(typeDefinition);
        return result;
    }

    public INavigationCommand<SymbolInfo> GetForMethod(IMethod method, string assemblyFilePath)
    {
        var result = new IlSpyMethodSymbolInfoCommand(method);
        return result;
    }

    public INavigationCommand<SymbolInfo> GetForEnumField(IField field, string projectAssemblyFilePath)
    {
        throw new System.NotImplementedException();
    }

    public INavigationCommand<SymbolInfo> GetForField(IField field, string assemblyFilePath)
    {
        var result = new IlSpyMemberSymbolInfoCommand(field);
        return result;
    }

    public INavigationCommand<SymbolInfo> GetForProperty(IProperty property, string assemblyFilePath)
    {
        var result = new IlSpyMemberSymbolInfoCommand(property);
        return result;
    }

    public INavigationCommand<SymbolInfo> GetForVariable(
        ILVariable variable,
        ITypeDefinition typeDefinition,
        SyntaxTree syntaxTree,
        string sourceText,
        string assemblyFilePath)
    {
        var result = new IlSpyVariableSymbolInfoCommand(variable);
        return result;
    }
}