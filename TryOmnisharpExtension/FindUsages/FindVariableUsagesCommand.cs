using System.ComponentModel.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using IlSpy.Analyzer.Extraction;
using TryOmnisharpExtension.FindUsages;

namespace TryOmnisharpExtension;

[Export(typeof(INavigationCommand<FindImplementationsResponse>))]
internal class FindVariableUsagesCommand : INavigationCommand<FindUsagesResponse>
{
    private readonly IlSpyVariableUsagesFinder _usagesFinder;
    private AstNode _variable;
    private ITypeDefinition _containingTypeDefinition;

    [ImportingConstructor]
    public FindVariableUsagesCommand(
        ITypeDefinition containingTypeDefinition,
        AstNode variable,
        IlSpyVariableUsagesFinder usagesFinder)
    {
        _containingTypeDefinition = containingTypeDefinition;
        _variable = variable;
        _usagesFinder = usagesFinder;
    }
        
    public async Task<FindUsagesResponse> Execute()
    {
        var metadataSources = await _usagesFinder.Run(
            _containingTypeDefinition,
            _variable);

        var result = new FindUsagesResponse();
            
        foreach (var metadataSource in metadataSources)
        {
            DecompileInfo decompileInfo = DecompileInfoMapper.MapFromMetadataSource(metadataSource);
            result.Implementations.Add(decompileInfo);
        }
            
        return result;
    }
}