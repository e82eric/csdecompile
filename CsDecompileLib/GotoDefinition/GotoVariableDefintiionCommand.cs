using System.Threading.Tasks;
using CsDecompileLib.FindUsages;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GotoDefinition;

class GotoVariableDefintiionCommand : INavigationCommand<DecompileGotoDefinitionResponse>
{
    private readonly IlSpyVariableDefintionFinder _finder;
    private readonly ITypeDefinition _containingTypeDefinition;
    private readonly AstNode _methodNode;
    private readonly ILVariable _variable;
    private readonly string _containingTypeSourceText;
    private readonly string _assemblyFileInfo;

    public GotoVariableDefintiionCommand(
        IlSpyVariableDefintionFinder finder,
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

    public Task<ResponsePacket<DecompileGotoDefinitionResponse>> Execute()
    {
        var defintion = _finder.Run(
            _containingTypeDefinition,
            _methodNode,
            _variable,
            _containingTypeSourceText);
            
        defintion.AssemblyFilePath = _assemblyFileInfo;
                
        var result = new DecompileGotoDefinitionResponse
        {
            Location = defintion,
            SourceText = _containingTypeSourceText,
            IsDecompiled = true
        };

        var response = new ResponsePacket<DecompileGotoDefinitionResponse>
        {
            Body = result,
            Success = true
        };
        
        return Task.FromResult(response);
    }
}