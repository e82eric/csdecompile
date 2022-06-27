using OmniSharp;

namespace TryOmnisharpExtension.ExternalAssemblies;

public class AddExternalAssemblyDirectoryRequest : IRequest
{
    public string DirectoryFilePath { get; set; }
}