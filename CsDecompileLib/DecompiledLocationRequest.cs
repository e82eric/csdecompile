namespace CsDecompileLib;

public class DecompiledLocationRequest : LocationRequest
{
    public string AssemblyFilePath { get; set; }
    public string ParentAssemblyFilePath { get; set; }
    public string ContainingTypeFullName { get; set; }
    public string AssemblyName { get; set; }
}