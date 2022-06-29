using System;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using TryOmnisharpExtension.FindUsages;
using TryOmnisharpExtension.IlSpy;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace TryOmnisharpExtension.FindImplementations;

public class GenericIlSpyFindImplementationsCommandFactory<TResponseType>
    where TResponseType : FindImplementationsResponse, new()
{
    private readonly ICommandFactory<INavigationCommand<TResponseType>> _commandCommandFactory;
    private readonly IDecompileWorkspace _decompileWorkspace;
    private readonly IlSpySymbolFinder _symbolFinder;

    public GenericIlSpyFindImplementationsCommandFactory(
        IlSpySymbolFinder symbolFinder,
        ICommandFactory<INavigationCommand<TResponseType>> commandCommandFactory,
        IDecompileWorkspace decompileWorkspace)
    {
        _symbolFinder = symbolFinder;
        _commandCommandFactory = commandCommandFactory;
        _decompileWorkspace = decompileWorkspace;
    }

    public async Task<INavigationCommand<TResponseType>> Find(DecompiledLocationRequest request)
    {
        var containingTypeDefinition = _symbolFinder.FindTypeDefinition(
            request.AssemblyFilePath,
            request.ContainingTypeFullName);
        
         (AstNode node, ICSharpCode.Decompiler.CSharp.Syntax.SyntaxTree, string sourceText) findNodeResult = _symbolFinder.FindNode(
            containingTypeDefinition,
            request.Line,
            request.Column);

        var parentResolveResult = findNodeResult.node.Parent.GetResolveResult();

        //TODO: This should be in a usages specific context
        if (parentResolveResult is ILVariableResolveResult)
        {
            var command = ((FindUsagesCommandFactory)_commandCommandFactory).GetForVariable(
                containingTypeDefinition,
                findNodeResult.node,
                findNodeResult.sourceText);
            return (INavigationCommand<TResponseType>)command;
        }

        var symbolAtLocation = _symbolFinder.FindSymbolFromNode(findNodeResult.node);

        if (symbolAtLocation is ITypeDefinition entity)
        {
            var ilSpyCommand = _commandCommandFactory.GetForType(
                entity,
                request.AssemblyFilePath);
            
            var symbol = await GetTypeSymbol(entity.FullName);
            if (symbol != null)
            {
                var roslynCommand = _commandCommandFactory.GetForInSource(symbol);
                var result = new EverywhereImplementationsCommand2<TResponseType>(roslynCommand, ilSpyCommand);

                return result;
            }

            return ilSpyCommand;
        }

        if (symbolAtLocation is IProperty property)
        {
            var ilSpyCommand = _commandCommandFactory.GetForProperty(property, request.AssemblyFilePath);
            var propertySymbol = await GetRoslynMemberSymbol<IPropertySymbol>(property);
            if (propertySymbol != null)
            {
                var roslynCommand = _commandCommandFactory.GetForInSource(propertySymbol);
                var result = new EverywhereImplementationsCommand2<TResponseType>(roslynCommand, ilSpyCommand);
                
                return result;
            }

            return ilSpyCommand;
        }
        
        if (symbolAtLocation is IField field)
        {
            var ilSpyCommand = _commandCommandFactory.GetForField(field, request.AssemblyFilePath);

            var roslynField = await GetRoslynMemberSymbol<IFieldSymbol>(field);
            if (roslynField != null)
            {
                var roslynCommand = _commandCommandFactory.GetForInSource(roslynField);

                var result = new EverywhereImplementationsCommand2<TResponseType>(roslynCommand, ilSpyCommand);
                return result;
            }
            return ilSpyCommand;
        }

        if (symbolAtLocation is IEvent eventSymbol)
        {
            var ilSpyCommand = _commandCommandFactory.GetForEvent(eventSymbol, request.AssemblyFilePath);
            var roslynEvent = await GetRoslynMemberSymbol<IEventSymbol>(eventSymbol);
            if (roslynEvent != null)
            {
                var roslynCommand = _commandCommandFactory.GetForInSource(roslynEvent);

                var result = new EverywhereImplementationsCommand2<TResponseType>(roslynCommand, ilSpyCommand);
                return result;
            }

            return ilSpyCommand;
        }

        if (symbolAtLocation is IMethod method)
        {
            var ilSpyCommand = _commandCommandFactory.GetForMethod(method, request.AssemblyFilePath);
            var symbol = await GetRoslynMemberSymbol<IMethodSymbol>(method, methodSymbol =>
            {
                var areSame = RoslynToIlSpyEqualityExtensions.AreSameMethod(methodSymbol, method);
                return areSame;
            });

            if (symbol != null)
            {
                var roslynCommand = _commandCommandFactory.GetForInSource(symbol);

                var result = new EverywhereImplementationsCommand2<TResponseType>(roslynCommand, ilSpyCommand);
                return result;
            }

            return ilSpyCommand;
        }
        
        return null;
    }
    
    private async Task<TRoslyn> GetRoslynMemberSymbol<TRoslyn>(IMember ilSpySymbol) where TRoslyn : ISymbol
    {
        var result = await GetRoslynMemberSymbol<TRoslyn>(ilSpySymbol, roslyn =>
        {
            var sameField = RoslynToIlSpyEqualityExtensions.AreMemberSymbol(roslyn, ilSpySymbol);
            return sameField;
        });
        return result;
    }
    
    private async Task<TRoslyn> GetRoslynMemberSymbol<TRoslyn>(IMember ilSpySymbol, Predicate<TRoslyn> areSame)
    {
        var declaringTypeFullName = ilSpySymbol?.DeclaringType?.FullName;
        if (declaringTypeFullName == null)
        {
            return default;
        }
        var roslynTypeSymbol = await GetTypeSymbol(declaringTypeFullName);
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

    private async Task<INamedTypeSymbol> GetTypeSymbol(string fullName)
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