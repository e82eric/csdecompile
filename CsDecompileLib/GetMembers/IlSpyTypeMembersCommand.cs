using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GetMembers;

internal class IlSpyTypeMembersCommand: INavigationCommand<LocationsResponse>
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
        
    public Task<ResponsePacket<LocationsResponse>> Execute()
    {
        var metadataSources = _usagesFinder.Run(
            _symbol);

        var body = new LocationsResponse();
            
        foreach (var metadataSource in metadataSources)
        {
            body.Locations.Add(metadataSource);
        }

        var result = ResponsePacket.Ok(body);
        return Task.FromResult(result);
    }
}