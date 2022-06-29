using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using TryOmnisharpExtension.FindImplementations;

namespace TryOmnisharpExtension.GetMembers;

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
        
    public Task<FindImplementationsResponse> Execute()
    {
        var metadataSources = _usagesFinder.Run(
            _symbol);

        var result = new FindImplementationsResponse();
            
        foreach (var metadataSource in metadataSources)
        {
            result.Implementations.Add(metadataSource);
        }
            
        return Task.FromResult(result);
    }
}