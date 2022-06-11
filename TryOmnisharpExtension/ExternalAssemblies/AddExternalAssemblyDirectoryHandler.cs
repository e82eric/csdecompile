using System.Composition;
using System.Threading.Tasks;
using OmniSharp.Mef;

namespace TryOmnisharpExtension.ExternalAssemblies;

[OmniSharpHandler(Endpoints.AddExternalAssemblyDirectory, Languages.Csharp), Shared]
public class AddExternalAssemblyDirectoryHandler : IRequestHandler<AddExternalAssemblyDirectoryRequest, AddExternalAssemblyDirectoryResponse>
{
    private readonly ExternalAssembliesWorkspace _externalAssembliesWorkspace;

    [ImportingConstructor]
    public AddExternalAssemblyDirectoryHandler(
        ExternalAssembliesWorkspace externalAssembliesWorkspace)
    {
        _externalAssembliesWorkspace = externalAssembliesWorkspace;
    }
    
    public Task<AddExternalAssemblyDirectoryResponse> Handle(AddExternalAssemblyDirectoryRequest request)
    {
        _externalAssembliesWorkspace.AddDirectory(request.DirectoryFilePath);
        var response = new AddExternalAssemblyDirectoryResponse() { Success = true };
        return Task.FromResult(response);
    }
}