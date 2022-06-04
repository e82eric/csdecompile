using OmniSharp;
using OmniSharp.Mef;

namespace TryOmnisharpExtension.Workspace;

[OmniSharpEndpoint(Endpoints.LoadAssemblies, typeof(LoadAssembliesRequest), typeof(LoadAssembliesResponse))]
public class LoadAssembliesRequest : IRequest
{
    public bool Load { get; set; }
}