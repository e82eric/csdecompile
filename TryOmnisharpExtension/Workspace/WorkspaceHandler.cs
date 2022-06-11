using System.Composition;
using System.Threading.Tasks;
using OmniSharp.Mef;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.Workspace;

[OmniSharpHandler(Endpoints.LoadAssemblies, Languages.Csharp), Shared]
public class WorkspaceHandler : IRequestHandler<LoadAssembliesRequest, LoadAssembliesResponse>
{
    private readonly IDecompileWorkspace _workspace;

    [ImportingConstructor]
    public WorkspaceHandler(IDecompileWorkspace workspace)
    {
        _workspace = workspace;
    }
    
    public Task<LoadAssembliesResponse> Handle(LoadAssembliesRequest request)
    {
        _workspace.LoadDlls();
        var result = new LoadAssembliesResponse();
        return Task.FromResult(result);
    }
}