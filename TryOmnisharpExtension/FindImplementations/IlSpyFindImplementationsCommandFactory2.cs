using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using OmniSharp;
using TryOmnisharpExtension.FindImplementations;
using TryOmnisharpExtension.FindUsages;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace TryOmnisharpExtension;

[Shared]
[Export]
public class IlSpyFindImplementationsCommandFactory2<ResponseType>
    where ResponseType : FindImplementationsResponse, new()
{
    private readonly ICommandFactory<INavigationCommand<ResponseType>> _commandCommandFactory;
    private readonly OmniSharpWorkspace _omniSharpWorkspace;
    private readonly IlSpySymbolFinder _symbolFinder;
    private List<Compilation> _projectCompilations;

    [ImportingConstructor]
    public IlSpyFindImplementationsCommandFactory2(
        OmniSharpWorkspace omniSharpWorkspace,
        IlSpySymbolFinder symbolFinder,
        ICommandFactory<INavigationCommand<ResponseType>> commandCommandFactory)
    {
        _omniSharpWorkspace = omniSharpWorkspace;
        _symbolFinder = symbolFinder;
        _commandCommandFactory = commandCommandFactory;
    }

    public async Task<INavigationCommand<ResponseType>> Find(DecompiledLocationRequest request)
    {
        var containingTypeDefinition = _symbolFinder.FindTypeDefinition(
            request.AssemblyFilePath,
            request.ContainingTypeFullName);
        
        var node = _symbolFinder.FindNode(
            containingTypeDefinition,
            request.Line,
            request.Column);

        var parentResolveResult = node.Parent.GetResolveResult();

        //TODO: This should be in a usages specific context
        if (parentResolveResult is ILVariableResolveResult)
        {
            var command = ((FindUsagesCommandFactory)_commandCommandFactory).GetForVariable(containingTypeDefinition, node);
            return (INavigationCommand<ResponseType>)command;
        }

        var symbolAtLocation = _symbolFinder.FindSymbolFromNode(node);

        //TODO: Can I move this to /loadassemblies
        if (_projectCompilations == null)
        {
            _projectCompilations = new List<Compilation>();
            foreach (var project in _omniSharpWorkspace.CurrentSolution.Projects)
            {
                try
                {
                    var compilation = await project.GetCompilationAsync();
                    _projectCompilations.Add(compilation);
                }
                catch (Exception)
                {
                }
            }
        }

        if (symbolAtLocation is ITypeDefinition entity)
        {
            var ilSpyCommand = _commandCommandFactory.GetForType(
                entity,
                request.AssemblyFilePath);
            
            var symbol = GetTypeSymbol(entity.FullName);
            if (symbol != null)
            {
                var roslynCommand = _commandCommandFactory.GetForInSource(symbol);
                var result = new EverywhereImplementationsCommand2<ResponseType>(roslynCommand, ilSpyCommand);

                return result;
            }

            return ilSpyCommand;
        }

        if (symbolAtLocation is IProperty property)
        {
            var ilSpyCommand = _commandCommandFactory.GetForProperty(property, request.AssemblyFilePath);
            var propertySymbol = GetRoslynMemberSymbol<IPropertySymbol>(property);
            if (propertySymbol != null)
            {
                var roslynCommand = _commandCommandFactory.GetForInSource(propertySymbol);
                var result = new EverywhereImplementationsCommand2<ResponseType>(roslynCommand, ilSpyCommand);
                
                return result;
            }

            return ilSpyCommand;
        }
        
        if (symbolAtLocation is IField field)
        {
            var ilSpyCommand = _commandCommandFactory.GetForField(field, request.AssemblyFilePath);

            var roslynField = GetRoslynMemberSymbol<IFieldSymbol>(field);
            if (roslynField != null)
            {
                var roslynCommand = _commandCommandFactory.GetForInSource(roslynField);

                var result = new EverywhereImplementationsCommand2<ResponseType>(roslynCommand, ilSpyCommand);
                return result;
            }
            return ilSpyCommand;
        }

        if (symbolAtLocation is IEvent eventSymbol)
        {
            var ilSpyCommand = _commandCommandFactory.GetForEvent(eventSymbol, request.AssemblyFilePath);
            var roslynEvent = GetRoslynMemberSymbol<IEventSymbol>(eventSymbol);
            if (roslynEvent != null)
            {
                var roslynCommand = _commandCommandFactory.GetForInSource(roslynEvent);

                var result = new EverywhereImplementationsCommand2<ResponseType>(roslynCommand, ilSpyCommand);
                return result;
            }

            return ilSpyCommand;
        }

        if (symbolAtLocation is IMethod method)
        {
            var ilSpyCommand = _commandCommandFactory.GetForMethod(method, request.AssemblyFilePath);
            var symbol = GetRoslynMemberSymbol<IMethodSymbol>(method, methodSymbol =>
            {
                var areSame = RoslynToIlSpyEqualityExtensions.AreSameMethod(methodSymbol, method);
                return areSame;
            });

            if (symbol != null)
            {
                var roslynCommand = _commandCommandFactory.GetForInSource(symbol);

                var result = new EverywhereImplementationsCommand2<ResponseType>(roslynCommand, ilSpyCommand);
                return result;
            }

            return ilSpyCommand;
        }
        
        return null;
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
        INamedTypeSymbol symbol = null;
        foreach (var compilation in _projectCompilations)
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