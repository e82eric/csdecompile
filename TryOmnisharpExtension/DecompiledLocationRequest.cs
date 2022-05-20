namespace TryOmnisharpExtension;

public class DecompiledLocationRequest : LocationRequest
{
    public string AssemblyFilePath { get; set; }
    public string ContainingTypeFullName { get; set; }
}