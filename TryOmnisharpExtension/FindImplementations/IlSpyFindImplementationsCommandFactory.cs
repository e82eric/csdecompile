using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using OmniSharp;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace TryOmnisharpExtension;

[Export]
public class IlSpyFindImplementationsCommandFactory
{
    private readonly ICommandFactory<IFindImplementationsCommand> _commandCommandFactory;
    private readonly OmniSharpWorkspace _omniSharpWorkspace;
    private readonly IlSpySymbolFinder _symbolFinder;

    [ImportingConstructor]
    public IlSpyFindImplementationsCommandFactory(
        OmniSharpWorkspace omniSharpWorkspace,
        IlSpySymbolFinder symbolFinder,
        ICommandFactory<IFindImplementationsCommand> commandCommandFactory)
    {
        _omniSharpWorkspace = omniSharpWorkspace;
        _symbolFinder = symbolFinder;
        _commandCommandFactory = commandCommandFactory;
    }
        
    public async Task<IFindImplementationsCommand> Find(DecompiledLocationRequest request)
    {
        var symbolAtLocation = await _symbolFinder.FindSymbolAtLocation(
            request.AssemblyFilePath,
            request.ContainingTypeFullName,
            request.Line,
            request.Column);
            
        var project =
            _omniSharpWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.OutputFilePath == request.AssemblyFilePath);
            
        var compilation = await project.GetCompilationAsync();

        if (symbolAtLocation is ITypeDefinition entity)
        {
            var symbol = compilation.GetTypeByMetadataName(entity.FullName);

            var roslynCommand =
                new RosylynFindImplementationsCommand(symbol, _omniSharpWorkspace);
                
            var ilSpyCommand = _commandCommandFactory.GetForType(
                entity,
                request.AssemblyFilePath);

            var result = new EverywhereImplementationsCommand(roslynCommand, ilSpyCommand);
            return result;
        }
            
        if (symbolAtLocation is IProperty property)
        {
            var symbol = compilation.GetTypeByMetadataName(property.DeclaringType.FullName);

            var roslynCommand =
                new RosylynFindImplementationsCommand(symbol, _omniSharpWorkspace);
                
            var ilSpyCommand = _commandCommandFactory.GetForProperty(property, request.AssemblyFilePath);
            var result = new EverywhereImplementationsCommand(roslynCommand, ilSpyCommand);
            return result;
        }

        if(symbolAtLocation is IMethod method)
        {
            var symbol = compilation.GetTypeByMetadataName(method.DeclaringType.FullName);

            var roslynCommand =
                new RosylynFindImplementationsCommand(symbol, _omniSharpWorkspace);
                
            var ilSpyCommand = _commandCommandFactory.GetForMethod(method, request.AssemblyFilePath);
            var result = new EverywhereImplementationsCommand(roslynCommand, ilSpyCommand);
            return result;
        }

        return null;
    }
}

[Shared]
[Export]
public class IlSpyFindImplementationsCommandFactory2<ResponseType> where ResponseType : FindImplementationsResponse, new()
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
                // new RosylynFindImplementationsCommand(symbol, _omniSharpWorkspace, request.AssemblyFilePath);
                
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
                // new RosylynFindImplementationsCommand(symbol, _omniSharpWorkspace, request.AssemblyFilePath);
                
            var ilSpyCommand = _commandCommandFactory.GetForProperty(property, request.AssemblyFilePath);
            var result = new EverywhereImplementationsCommand2<ResponseType>(roslynCommand, ilSpyCommand);
            return result;
        }
        
        if (symbolAtLocation is IEvent eventSymbol)
        {
            var symbol = GetSymbol(eventSymbol.DeclaringType.FullName);

            var roslynCommand = _commandCommandFactory.GetForInSource(symbol);
                // new RosylynFindImplementationsCommand(symbol, _omniSharpWorkspace, request.AssemblyFilePath);
                
            var ilSpyCommand = _commandCommandFactory.GetForEvent(eventSymbol, request.AssemblyFilePath);
            var result = new EverywhereImplementationsCommand2<ResponseType>(roslynCommand, ilSpyCommand);
            return result;
        }

        if(symbolAtLocation is IMethod method)
        {
            var symbol = GetSymbol(method.DeclaringType.FullName);
            INavigationCommand<ResponseType> roslynCommand = null;
            if (symbol is INamedTypeSymbol)
            {
                var methodSymbol = ((INamedTypeSymbol)symbol).GetMembers().FirstOrDefault(m => m.Name == method.Name);
                roslynCommand = _commandCommandFactory.GetForInSource(methodSymbol);
                    
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