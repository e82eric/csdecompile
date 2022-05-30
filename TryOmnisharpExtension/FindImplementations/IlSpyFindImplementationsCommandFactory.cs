using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using OmniSharp;
using TryOmnisharpExtension.IlSpy;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace TryOmnisharpExtension;

public class IlSpyFindImplementationsCommandFactory : IDecompilerCommandFactory<INavigationCommand<FindImplementationsResponse>>
{
    private readonly IlSpyBaseTypeUsageFinder2 _typeFinder;
    private readonly IlSpyMethodImplementationFinder _methodImplementationFinder;
    private readonly IlSpyPropertyImplementationFinder _propertyImplementationFinder;
    private readonly IlSpyEventImplementationFinder _eventImplementationFinder;

    [ImportingConstructor]
    public IlSpyFindImplementationsCommandFactory(
        IlSpyBaseTypeUsageFinder2 typeFinder,
        IlSpyMethodImplementationFinder methodImplementationFinder,
        IlSpyPropertyImplementationFinder propertyImplementationFinder,
        IlSpyEventImplementationFinder eventImplementationFinder)
    {
        _typeFinder = typeFinder;
        _methodImplementationFinder = methodImplementationFinder;
        _propertyImplementationFinder = propertyImplementationFinder;
        _eventImplementationFinder = eventImplementationFinder;
    }

    public INavigationCommand<FindImplementationsResponse> GetForType(ITypeDefinition typeDefinition, string projectAssemblyFilePath)
    {
        return new FindTypeImplementationsCommand(
            projectAssemblyFilePath,
            typeDefinition,
            _typeFinder);
    }

    public INavigationCommand<FindImplementationsResponse> GetForMethod(IMethod method, string projectAssemblyFilePath)
    {
        return new FindMethodImplementationsCommand(
            projectAssemblyFilePath,
            method,
            _methodImplementationFinder);
    }

    public INavigationCommand<FindImplementationsResponse> GetForProperty(IProperty property, string projectAssemblyFilePath)
    {
        return new FindPropertyImplementationsCommand(
            projectAssemblyFilePath,
            property,
            _propertyImplementationFinder);
    }
        
    public INavigationCommand<FindImplementationsResponse> GetForEvent(IEvent eventSymbol, string projectAssemblyFilePath)
    {
        return new FindEventImplementationsCommand(
            projectAssemblyFilePath,
            eventSymbol,
            _eventImplementationFinder);
    }
}

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
        var symbolAtLocation = await _symbolFinder.FindSymbolAtLocation(
            request.AssemblyFilePath,
            request.ContainingTypeFullName,
            request.Line,
            request.Column);

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
            var symbol = GetSymbol(entity.FullName);

            var roslynCommand = _commandCommandFactory.GetForInSource(symbol);

            var ilSpyCommand = _commandCommandFactory.GetForType(
                entity,
                request.AssemblyFilePath);

            var result = new EverywhereImplementationsCommand2<ResponseType>(roslynCommand, ilSpyCommand);
            return result;
        }

        if (symbolAtLocation is IProperty property)
        {
            var symbol = GetSymbol(property.DeclaringType.FullName);

            var roslynCommand = _commandCommandFactory.GetForInSource(symbol);

            var ilSpyCommand = _commandCommandFactory.GetForProperty(property, request.AssemblyFilePath);
            var result = new EverywhereImplementationsCommand2<ResponseType>(roslynCommand, ilSpyCommand);
            return result;
        }

        if (symbolAtLocation is IEvent eventSymbol)
        {
            var symbol = GetSymbol(eventSymbol.DeclaringType.FullName);

            var roslynCommand = _commandCommandFactory.GetForInSource(symbol);

            var ilSpyCommand = _commandCommandFactory.GetForEvent(eventSymbol, request.AssemblyFilePath);
            var result = new EverywhereImplementationsCommand2<ResponseType>(roslynCommand, ilSpyCommand);
            return result;
        }

        if (symbolAtLocation is IMethod method)
        {
            var symbol = GetSymbol(method.DeclaringType.FullName);
            INavigationCommand<ResponseType> roslynCommand = null;
            if (symbol is INamedTypeSymbol roslynType)
            {
                IMethodSymbol foundRoslynMethod = null;
                foreach (var roslynMember in roslynType.GetMembers())
                {
                    if (roslynMember is IMethodSymbol roslynMethod)
                    {
                        var areSame = RoslynToIlSpyEqualityExtensions.AreSameMethod(roslynMethod, method);
                        if (areSame)
                        {
                            foundRoslynMethod = roslynMethod;
                            break;
                        }
                    }
                }

                roslynCommand = _commandCommandFactory.GetForInSource(foundRoslynMethod);
            }

            var ilSpyCommand = _commandCommandFactory.GetForMethod(method, request.AssemblyFilePath);
            var result = new EverywhereImplementationsCommand2<ResponseType>(roslynCommand, ilSpyCommand);
            return result;
        }

        return null;
    }

    private ISymbol GetSymbol(string fullName)
    {
        ISymbol symbol = null;
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