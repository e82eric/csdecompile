using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using CsDecompileLib.FindImplementations;

namespace CsDecompileLib.FindUsages;

internal class FindVariableUsagesCommand : INavigationCommand<FindImplementationsResponse>
{
    private readonly IlSpyVariableUsagesFinder _usagesFinder;
    private readonly string _sourceText;
    private readonly AstNode _variable;
    private readonly ITypeDefinition _containingTypeDefinition;

    public FindVariableUsagesCommand(
        ITypeDefinition containingTypeDefinition,
        AstNode variable,
        IlSpyVariableUsagesFinder usagesFinder,
        string sourceText)
    {
        _containingTypeDefinition = containingTypeDefinition;
        _variable = variable;
        _usagesFinder = usagesFinder;
        _sourceText = sourceText;
    }
        
    public Task<ResponsePacket<FindImplementationsResponse>> Execute()
    {
        var metadataSources = _usagesFinder.Run(
            _containingTypeDefinition,
            _variable,
            _sourceText);

        var body = new FindImplementationsResponse();
            
        foreach (var metadataSource in metadataSources)
        {
            // DecompileInfo decompileInfo = DecompileInfoMapper.MapFromMetadataSource(metadataSource);
            body.Implementations.Add(metadataSource);
        }

        var result = ResponsePacket.Ok(body);

        return Task.FromResult(result);
    }
}