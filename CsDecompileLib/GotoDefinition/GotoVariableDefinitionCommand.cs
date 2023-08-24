using System.Threading.Tasks;
using CsDecompileLib.FindUsages;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GotoDefinition;

class GotoVariableDefinitionCommand : INavigationCommand<LocationsResponse>
{
    private readonly IlSpyVariableDefinitionFinder _finder;
    private readonly ITypeDefinition _containingTypeDefinition;
    private readonly AstNode _methodNode;
    private readonly ILVariable _variable;
    private readonly string _containingTypeSourceText;
    private readonly string _assemblyFileInfo;

    public GotoVariableDefinitionCommand(
        IlSpyVariableDefinitionFinder finder,
        ITypeDefinition containingTypeDefinition,
        AstNode methodNode,
        ILVariable variable,
        string containingTypeSourceText,
        string assemblyFileInfo)
    {
        _finder = finder;
        _containingTypeDefinition = containingTypeDefinition;
        _methodNode = methodNode;
        _variable = variable;
        _containingTypeSourceText = containingTypeSourceText;
        _assemblyFileInfo = assemblyFileInfo;
    }

    public Task<ResponsePacket<LocationsResponse>> Execute()
    {
        var definition = _finder.Run(
            _containingTypeDefinition,
            _methodNode,
            _variable,
            _containingTypeSourceText);
            
        definition.AssemblyFilePath = _assemblyFileInfo;
                
        var result = new LocationsResponse
        {
            Locations = { definition },
        };

        var response = new ResponsePacket<LocationsResponse>
        {
            Body = result,
            Success = true
        };
        
        return Task.FromResult(response);
    }
}