using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace TryOmnisharpExtension.GetMembers;

internal class IlSpyTypeMembersCommand: INavigationCommand<GetTypeMembersResponse>
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
        
    public Task<GetTypeMembersResponse> Execute()
    {
        var metadataSources = _usagesFinder.Run(
            _symbol);

        var result = new GetTypeMembersResponse();
            
        foreach (var metadataSource in metadataSources)
        {
            result.Implementations.Add(metadataSource);
        }
            
        return Task.FromResult(result);
    }
}