using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.FindUsages;

namespace TryOmnisharpExtension.GotoDefinition;

class GotoVariableDefintiionCommand : INavigationCommand<DecompileGotoDefinitionResponse>
{
    private readonly IlSpyVariableDefintionFinder _finder;
    private readonly ITypeDefinition _containingTypeDefinition;
    private readonly SyntaxTree _containingTypeSyntaxTree;
    private readonly ILVariable _variable;
    private readonly string _containingTypeSourceText;
    private readonly string _assemblyFileInfo;

    public GotoVariableDefintiionCommand(
        IlSpyVariableDefintionFinder finder,
        ITypeDefinition containingTypeDefinition,
        SyntaxTree containingTypeSyntaxTree,
        ILVariable variable,
        string containingTypeSourceText,
        string assemblyFileInfo)
    {
        _finder = finder;
        _containingTypeDefinition = containingTypeDefinition;
        _containingTypeSyntaxTree = containingTypeSyntaxTree;
        _variable = variable;
        _containingTypeSourceText = containingTypeSourceText;
        _assemblyFileInfo = assemblyFileInfo;
    }

    public Task<ResponsePacket<DecompileGotoDefinitionResponse>> Execute()
    {
        var defintion = _finder.Run(
            _containingTypeDefinition,
            _containingTypeSyntaxTree,
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