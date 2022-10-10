using System;
using CsDecompileLib.IlSpy;
using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;
using TypeKind = ICSharpCode.Decompiler.TypeSystem.TypeKind;

namespace CsDecompileLib.FindImplementations;

public class EveryWhereFindImplementationsCommandFactory<TResponseType>
    : ICommandFactory<INavigationCommand<TResponseType>> where TResponseType : FindImplementationsResponse, new()
{
    private readonly ICommandFactory<INavigationCommand<TResponseType>> _commandCommandFactory;
    private readonly IDecompileWorkspace _decompileWorkspace;

    public EveryWhereFindImplementationsCommandFactory(
        ICommandFactory<INavigationCommand<TResponseType>> commandCommandFactory,
        IDecompileWorkspace decompileWorkspace)
    {
        _commandCommandFactory = commandCommandFactory;
        _decompileWorkspace = decompileWorkspace;
    }
    
    public INavigationCommand<TResponseType> GetForType(ITypeDefinition type, string projectAssemblyFilePath)
    {
        var ilSpyCommand = _commandCommandFactory.GetForType(
            type,
            projectAssemblyFilePath);
        
        var symbol = GetTypeSymbol(type.FullName);
        if (symbol != null)
        {
            var roslynCommand = _commandCommandFactory.GetForInSource(symbol);
            var result = new EverywhereImplementationsCommand<TResponseType>(roslynCommand, ilSpyCommand);

            return result;
        }

        return ilSpyCommand;
    }

    public INavigationCommand<TResponseType> GetForMethod(IMethod method, string projectAssemblyFilePath)
    {
        var ilSpyCommand = _commandCommandFactory.GetForMethod(method, projectAssemblyFilePath);
        var symbol = GetRoslynMemberSymbol<IMethodSymbol>(method, methodSymbol =>
        {
            var areSame = RoslynToIlSpyEqualityExtensions.AreSameMethod(methodSymbol, method);
            return areSame;
        });

        if (symbol != null)
        {
            var roslynCommand = _commandCommandFactory.GetForInSource(symbol);

            var result = new EverywhereImplementationsCommand<TResponseType>(roslynCommand, ilSpyCommand);
            return result;
        }

        return ilSpyCommand;
    }

    public INavigationCommand<TResponseType> GetForProperty(IProperty property, string projectAssemblyFilePath)
    {
        var ilSpyCommand = _commandCommandFactory.GetForProperty(property, projectAssemblyFilePath);
        var propertySymbol = GetRoslynMemberSymbol<IPropertySymbol>(property);
        if (propertySymbol != null)
        {
            var roslynCommand = _commandCommandFactory.GetForInSource(propertySymbol);
            var result = new EverywhereImplementationsCommand<TResponseType>(roslynCommand, ilSpyCommand);
            
            return result;
        }

        return ilSpyCommand;
    }

    public INavigationCommand<TResponseType> GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
    {
        var ilSpyCommand = _commandCommandFactory.GetForEvent(eventSymbol, projectAssemblyFilePath);
        var roslynEvent = GetRoslynMemberSymbol<IEventSymbol>(eventSymbol);
        if (roslynEvent != null)
        {
            var roslynCommand = _commandCommandFactory.GetForInSource(roslynEvent);

            var result = new EverywhereImplementationsCommand<TResponseType>(roslynCommand, ilSpyCommand);
            return result;
        }

        return ilSpyCommand;
    }

    public INavigationCommand<TResponseType> GetForEnumField(IField field, string projectAssemblyFilePath)
    {
        throw new NotImplementedException();
    }

    public INavigationCommand<TResponseType> GetForField(IField field, string projectAssemblyFilePath)
    {
        INavigationCommand<TResponseType> ilSpyCommand;
        if (field.DeclaringType.Kind == TypeKind.Enum)
        {
            ilSpyCommand = _commandCommandFactory.GetForEnumField(field, projectAssemblyFilePath);
        }
        else
        {
            ilSpyCommand = _commandCommandFactory.GetForField(field, projectAssemblyFilePath);
        }

        var roslynField = GetRoslynMemberSymbol<IFieldSymbol>(field);
        if (roslynField != null)
        {
            var roslynCommand = _commandCommandFactory.GetForInSource(roslynField);

            var result = new EverywhereImplementationsCommand<TResponseType>(roslynCommand, ilSpyCommand);
            return result;
        }
        return ilSpyCommand;
    }

    public INavigationCommand<TResponseType> GetForInSource(ISymbol roslynSymbol)
    {
        throw new NotImplementedException();
    }

    public INavigationCommand<TResponseType> GetForFileNotFound(string filePath)
    {
        throw new NotImplementedException();
    }

    public INavigationCommand<TResponseType> SymbolNotFoundAtLocation(string filePath, int line, int column)
    {
        throw new NotImplementedException();
    }

    public INavigationCommand<TResponseType> GetForVariable(ILVariable variable, ITypeDefinition typeDefinition, AstNode methodNode,
        string sourceText, string assemblyFilePath)
    {
        var result = _commandCommandFactory.GetForVariable(
            variable,
            typeDefinition,
            methodNode,
            sourceText,
            assemblyFilePath);
        return result;
    }
    
    private TRoslyn GetRoslynMemberSymbol<TRoslyn>(IMember ilSpySymbol) where TRoslyn : ISymbol
    {
        var result = GetRoslynMemberSymbol<TRoslyn>(ilSpySymbol, roslyn =>
        {
            var sameField = RoslynToIlSpyEqualityExtensions.AreMemberSymbol(roslyn, ilSpySymbol);
            return sameField;
        });
        return result;
    }
    
    private TRoslyn GetRoslynMemberSymbol<TRoslyn>(IMember ilSpySymbol, Predicate<TRoslyn> areSame)
    {
        var declaringTypeFullName = ilSpySymbol?.DeclaringType?.FullName;
        if (declaringTypeFullName == null)
        {
            return default;
        }
        var roslynTypeSymbol = GetTypeSymbol(declaringTypeFullName);
        if (roslynTypeSymbol == null)
        {
            return default;
        }
        var typePublicMembers = roslynTypeSymbol.GetMembers();
        foreach (var publicMember in typePublicMembers)
        {
            if (publicMember is TRoslyn roslynMember)
            {
                var sameField = areSame(roslynMember);
                if (sameField)
                {
                    return roslynMember;
                }
            }
        }

        return default;
    }

    private INamedTypeSymbol GetTypeSymbol(string fullName)
    {
        var compilations = _decompileWorkspace.GetProjectCompilations();
        INamedTypeSymbol symbol = null;
        foreach (var compilation in compilations)
        {
            symbol = compilation.GetTypeByMetadataName(fullName);
            if (symbol != null)
            {
                break;
            }
        }

        return symbol;
    }
}