using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindImplementations;

public class IlSpyExternalAssembliesCommandFactory<TResponseType> where TResponseType : FindImplementationsResponse, new()
{
    private readonly IDecompilerCommandFactory<INavigationCommand<TResponseType>> _commandCommandFactory;
    private readonly IlSpySymbolFinder _symbolFinder;

    public IlSpyExternalAssembliesCommandFactory(
        IlSpySymbolFinder symbolFinder,
        IDecompilerCommandFactory<INavigationCommand<TResponseType>> commandCommandFactory)
    {
        _symbolFinder = symbolFinder;
        _commandCommandFactory = commandCommandFactory;
    }
        
    public INavigationCommand<TResponseType> Find(DecompiledLocationRequest request)
    {
        var symbolAtLocation = _symbolFinder.FindSymbolAtLocation(
            request.AssemblyFilePath,
            request.ContainingTypeFullName,
            request.Line,
            request.Column);

        if (symbolAtLocation is ITypeDefinition entity)
        {
            var ilSpyCommand = _commandCommandFactory.GetForType(
                entity,
                request.AssemblyFilePath);

            return ilSpyCommand;
        }
            
        if (symbolAtLocation is IProperty property)
        {
            var ilSpyCommand = _commandCommandFactory.GetForProperty(property, request.AssemblyFilePath);
            return ilSpyCommand;
        }
        
        if (symbolAtLocation is IEvent eventSymbol)
        {
            var ilSpyCommand = _commandCommandFactory.GetForEvent(eventSymbol, request.AssemblyFilePath);
            return ilSpyCommand;
        }

        if(symbolAtLocation is IMethod method)
        {
            var ilSpyCommand = _commandCommandFactory.GetForMethod(method, request.AssemblyFilePath);
            return ilSpyCommand;
        }

        return null;
    }
}