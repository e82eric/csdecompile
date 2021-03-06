using System.ComponentModel.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.FindImplementations;

namespace TryOmnisharpExtension.FindUsages;

[Export(typeof(INavigationCommand<FindImplementationsResponse>))]
internal class FindVariableUsagesCommand : INavigationCommand<FindUsagesResponse>
{
    private readonly IlSpyVariableUsagesFinder _usagesFinder;
    private readonly string _sourceText;
    private readonly AstNode _variable;
    private readonly ITypeDefinition _containingTypeDefinition;

    [ImportingConstructor]
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
        
    public Task<FindUsagesResponse> Execute()
    {
        var metadataSources = _usagesFinder.Run(
            _containingTypeDefinition,
            _variable,
            _sourceText);

        var result = new FindUsagesResponse();
            
        foreach (var metadataSource in metadataSources)
        {
            // DecompileInfo decompileInfo = DecompileInfoMapper.MapFromMetadataSource(metadataSource);
            result.Implementations.Add(metadataSource);
        }
            
        return Task.FromResult(result);
    }
}