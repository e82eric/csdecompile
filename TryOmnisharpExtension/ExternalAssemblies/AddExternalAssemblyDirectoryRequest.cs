using OmniSharp;
using OmniSharp.Mef;

namespace TryOmnisharpExtension.ExternalAssemblies;

[OmniSharpEndpoint(Endpoints.AddExternalAssemblyDirectory, typeof(AddExternalAssemblyDirectoryRequest), typeof(AddExternalAssemblyDirectoryResponse))]
public class AddExternalAssemblyDirectoryRequest : IRequest
{
    public string DirectoryFilePath { get; set; }
}