using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using CsDecompileLib.FindImplementations;

namespace CsDecompileLib.GetMembers;

internal class IlSpyTypeMembersCommand: INavigationCommand<FindImplementationsResponse>
{
    private readonly ITypeDefinition _symbol;
    private readonly IlSpyTypeMembersFinder _usagesFinder;

    public IlSpyTypeMembersCommand(
        ITypeDefinition symbol,
        IlSpyTypeMembersFinder usagesFinder)
    {
        _symbol = symbol;
        _usagesFinder = usagesFinder;
    }
        
    public Task<ResponsePacket<FindImplementationsResponse>> Execute()
    {
        var metadataSources = _usagesFinder.Run(
            _symbol);

        var body = new FindImplementationsResponse();
            
        foreach (var metadataSource in metadataSources)
        {
            body.Implementations.Add(metadataSource);
        }

        var result = ResponsePacket.Ok(body);
        return Task.FromResult(result);
    }
}