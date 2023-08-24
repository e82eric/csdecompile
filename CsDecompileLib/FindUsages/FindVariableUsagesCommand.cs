using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace CsDecompileLib.FindUsages;

internal class FindVariableUsagesCommand : INavigationCommand<LocationsResponse>
{
    private readonly IlSpyVariableUsagesFinder _usagesFinder;
    private readonly string _sourceText;
    private readonly AstNode _methodBodyNode;
    private readonly ILVariable _variable;
    private readonly ITypeDefinition _containingTypeDefinition;

    public FindVariableUsagesCommand(
        ITypeDefinition containingTypeDefinition,
        AstNode methodBody,
        ILVariable variable,
        IlSpyVariableUsagesFinder usagesFinder,
        string sourceText)
    {
        _containingTypeDefinition = containingTypeDefinition;
        _variable = variable;
        _usagesFinder = usagesFinder;
        _sourceText = sourceText;
        _methodBodyNode = methodBody;
    }
        
    public Task<ResponsePacket<LocationsResponse>> Execute()
    {
        var metadataSources = _usagesFinder.Run(
            _containingTypeDefinition,
            _methodBodyNode,
            _variable,
            _sourceText);

        var body = new LocationsResponse();
            
        foreach (var metadataSource in metadataSources)
        {
            body.Locations.Add(metadataSource);
        }

        var result = ResponsePacket.Ok(body);

        return Task.FromResult(result);
    }
}