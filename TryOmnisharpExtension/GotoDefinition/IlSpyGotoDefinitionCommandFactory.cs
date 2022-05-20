using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension
{
    // [Export]
    // public class IlSpyGotToDefinitionCommandFactory
    // {
    //     private readonly GotoDefinitionCommandFactory _commandCommandFactory;
    //     private readonly IlSpySymbolFinder _spySymbolFinder;
    //
    //     [ImportingConstructor]
    //     public IlSpyGotToDefinitionCommandFactory(
    //         GotoDefinitionCommandFactory commandCommandFactory,
    //         IlSpySymbolFinder spySymbolFinder)
    //     {
    //         _commandCommandFactory = commandCommandFactory;
    //         _spySymbolFinder = spySymbolFinder;
    //     }
    //     
    //     public async Task<IGotoDefinitionCommand> Find(DecompileGotoDefinitionFromDecompileRequest request)
    //     {
    //         var symbolAtLocation = await _spySymbolFinder.FindSymbolAtLocation(
    //             request.AssemblyFilePath,
    //             request.ContainingTypeFullName,
    //             request.Line,
    //             request.Column);
    //
    //         if (symbolAtLocation is ITypeDefinition entity)
    //         {
    //             var result = _commandCommandFactory.GetType(entity, request.AssemblyFilePath);
    //             return result;
    //         }
    //
    //         if (symbolAtLocation is IProperty property)
    //         {
    //             var result = _commandCommandFactory.GetProperty(property, request.AssemblyFilePath);
    //             return result;
    //         }
    //
    //         if(symbolAtLocation is IMethod method)
    //         {
    //             var result = _commandCommandFactory.GetMethod(method, request.AssemblyFilePath);
    //             return result;
    //         }
    //
    //         return null;
    //     }
    // }
}